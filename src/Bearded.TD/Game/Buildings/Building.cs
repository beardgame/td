using System;
using System.Collections.Generic;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Buildings
{
    abstract class Building : GameObject
    {
        private readonly BuildingBlueprint blueprint;
        private PositionedFootprint footprint;

        public BuildProcessManager BuildManager { get; private set; }
        
        public Position2 Position { get; private set; }
        protected int Health { get; private set; }
        public IEnumerable<Tile<TileInfo>> OccupiedTiles => footprint.OccupiedTiles;

        protected List<Component> Components { get; } = new List<Component>();

        protected Building(BuildingBlueprint blueprint, PositionedFootprint footprint)
        {
            if (!footprint.IsValid) throw new ArgumentOutOfRangeException();

            this.blueprint = blueprint;
            this.footprint = footprint;
            Health = 1;
            BuildManager = new BuildProcessManager(this, blueprint);
        }

        public void Damage(int damage)
        {
            Health -= damage;
            OnDamaged();
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Position = footprint.CenterPosition;
            OccupiedTiles.ForEach((tile) => Game.Geometry.SetBuilding(tile, this));
            
        }

        private void onCompleted()
        {
            blueprint.GetComponents().ForEach(Components.Add);
            Components.ForEach(c => c.OnAdded(this));
            BuildManager = null;
        }

        protected override void OnDelete()
        {
            OccupiedTiles.ForEach((tile) => Game.Geometry.SetBuilding(tile, null));
        }

        protected virtual void OnDamaged()
        { }

        public override void Update(TimeSpan elapsedTime)
        {
            foreach (var component in Components)
                component.Update(elapsedTime);
        }

        public override void Draw(GeometryManager geometries)
        {
            foreach (var component in Components)
                component.Draw(geometries);
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
                building.Health += expectedHealthGiven - healthGiven;
                healthGiven = expectedHealthGiven;
            }
        }
    }
}
