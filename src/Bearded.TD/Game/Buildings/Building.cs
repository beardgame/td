using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.gameplay;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Buildings
{
    abstract class Building : GameObject, IIdable<Building>
    {
        private readonly BuildingBlueprint blueprint;
        private PositionedFootprint footprint;

        public Id<Building> Id { get; }

        public BuildProcessManager BuildManager { get; private set; }
        
        public Faction Faction { get; }
        public Position2 Position { get; private set; }
        public int Health { get; private set; }
        public IEnumerable<Tile<TileInfo>> OccupiedTiles => footprint.OccupiedTiles;

        protected List<Component> Components { get; } = new List<Component>();

        public event VoidEventHandler Damaged;

        protected Building(Id<Building> id, BuildingBlueprint blueprint, PositionedFootprint footprint, Faction faction)
        {
            if (!footprint.IsValid) throw new ArgumentOutOfRangeException();

            Id = id;
            this.blueprint = blueprint;
            this.footprint = footprint;
            Faction = faction;
            Health = 1;
            BuildManager = new BuildProcessManager(this, blueprint);
        }

        public void Damage(int damage)
        {
            Health -= damage;
            Damaged?.Invoke();
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);

            Position = footprint.CenterPosition;
            OccupiedTiles.ForEach(tile =>
            {
                Game.Geometry.SetBuilding(tile, this);
                Game.Navigator.AddBackupSink(tile);
            });
        }

        private void onCompleted()
        {
            blueprint.GetComponents().ForEach(Components.Add);
            Components.ForEach(c => c.OnAdded(this));
            BuildManager = null;
        }

        protected override void OnDelete()
        {
            OccupiedTiles.ForEach(tile =>
            {
                Game.Geometry.SetBuilding(tile, null);
                Game.Navigator.RemoveSink(tile);
            });

            //TODO: abort resource consumption if still being built
        }

        public override void Update(TimeSpan elapsedTime)
        {
            foreach (var component in Components)
                component.Update(elapsedTime);

            if (Health <= 0)
            {
                this.Sync(KillBuilding.Command, this);
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            var alpha = (float)(BuildManager?.CurrentProgressFraction * 0.9 ?? 1);
            geo.Color = Color.Blue * alpha;

            foreach (var tile in footprint.OccupiedTiles)
                geo.DrawCircle(Game.Level.GetPosition(tile).NumericValue, HexagonSide, true, 6);

            foreach (var component in Components)
                component.Draw(geometries);
            
            geometries.PointLight.Draw(Position.NumericValue.WithZ(3), 3 + 2 * alpha, Color.Orange);
        }

        public bool HasComponentOfType<T>()
        {
            return Components.OfType<T>().FirstOrDefault() != null;
        }

        public class BuildProcessManager
        {
            private readonly Building building;
            private readonly BuildingBlueprint blueprint;
            private double buildProcess;
            private int healthGiven = 1;

            public double ResourcesStillNeeded => blueprint.ResourceCost - buildProcess;
            public double CurrentProgressFraction => buildProcess / blueprint.ResourceCost;

            public BuildProcessManager(Building building, BuildingBlueprint blueprint)
            {
                this.building = building;
                this.blueprint = blueprint;
            }

            public void Progress(ResourceGrant resources)
            {
                if (ResourcesStillNeeded <= 0 || building.Deleted) return;

                if (resources.ReachedCapacity)
                {
                    buildProcess = blueprint.ResourceCost;
                    building.Health += blueprint.MaxHealth - healthGiven;
                    building.onCompleted();
                    return;
                }

                buildProcess += resources.Amount;
                var expectedHealthGiven = (int)(CurrentProgressFraction * blueprint.MaxHealth);
                if (expectedHealthGiven < healthGiven) return;
                building.Health += expectedHealthGiven - healthGiven;
                healthGiven = expectedHealthGiven;
            }
        }
    }
}
