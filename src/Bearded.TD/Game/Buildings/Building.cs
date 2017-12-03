using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.World;
using Bearded.TD.Meta;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Model;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using static Bearded.TD.Constants.Game.World;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Buildings
{
    partial class Building : PlacedBuildingBase<Building>, IIdable<Building>, IDamageable, IFactioned
    {
        private static readonly Dictionary<SelectionState, Color> drawColors = new Dictionary<SelectionState, Color>
        {
            {SelectionState.Default, Color.Blue},
            {SelectionState.Focused, Color.DarkBlue},
            {SelectionState.Selected, Color.RoyalBlue}
        };

        public Id<Building> Id { get; }
        
        public int Health { get; private set; }
        private bool isCompleted;
        private double buildProgress;

        public event VoidEventHandler Damaged;

        public WorkerTask WorkerTask
        {
            get
            {
                if (isCompleted)
                    throw new Exception("Cannot create a worker task for a completed building.");
                return new BuildingWorkerTask(this, Blueprint);
            }
        }

        public Building(Id<Building> id, BuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
            Id = id;
            Health = 1;
        }

        protected override IEnumerable<IComponent<Building>> InitialiseComponents()
            => Blueprint.GetComponents();

        public void Damage(int damage)
        {
            if (!UserSettings.Instance.Debug.InvulnerableBuildings)
                Health -= damage;
            Damaged?.Invoke();
        }

        protected override void OnAdded()
        {
            Game.IdAs(this);

            OccupiedTiles.ForEach(tile =>
            {
                Game.Geometry.SetBuilding(tile, this);
                Game.Navigator.AddBackupSink(tile);
            });
            
            base.OnAdded();
        }

        public void ResetToComplete()
        {
            Health = Blueprint.MaxHealth;
            if (!isCompleted)
                onCompleted();
        }

        private void onCompleted()
        {
            isCompleted = true;
        }

        protected override void OnDelete()
        {
            OccupiedTiles.ForEach(tile =>
            {
                Game.Geometry.SetBuilding(tile, null);
                Game.Navigator.RemoveSink(tile);
            });

            base.OnDelete();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            if (Health <= 0)
            {
                this.Sync(KillBuilding.Command);
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            var alpha = isCompleted ? 1 : (float)(buildProgress * 0.9);
            geo.Color = drawColors[SelectionState] * alpha;

            foreach (var tile in Footprint.OccupiedTiles)
                geo.DrawCircle(Game.Level.GetPosition(tile).NumericValue, HexagonSide, true, 6);
            
            base.Draw(geometries);

            geometries.PointLight.Draw(Position.NumericValue.WithZ(3), 3 + 2 * alpha, Color.Orange * 0.2f);
        }
    }
}
