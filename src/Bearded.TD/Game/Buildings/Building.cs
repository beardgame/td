using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Buildings
{
    [ComponentOwner]
    class Building : PlacedBuildingBase<Building>, IIdable<Building>
    {
        private static readonly Dictionary<SelectionState, Color> drawColors = new Dictionary<SelectionState, Color>
        {
            {SelectionState.Default, Color.Blue},
            {SelectionState.Focused, Color.DarkBlue},
            {SelectionState.Selected, Color.RoyalBlue}
        };

        public Id<Building> Id { get; }

        public bool IsCompleted { get; private set; }
        private double buildProgress;

        public event VoidEventHandler Completing;
        public event GenericEventHandler<int> Damaged;
        public event GenericEventHandler<int> HealthAdded;

        public Building(Id<Building> id, IBuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
            Id = id;
        }

        protected override IEnumerable<IComponent<Building>> InitialiseComponents()
            => Blueprint.GetComponentsForBuilding();

        public void Damage(int damage)
        {
            Damaged?.Invoke(damage);
        }

        protected override void OnAdded()
        {
            Game.IdAs(this);

            OccupiedTiles.ForEach(tile => Game.Navigator.AddBackupSink(tile));

            base.OnAdded();
        }

        public void SetBuildProgress(double newBuildProgress, int healthAdded)
        {
            DebugAssert.State.Satisfies(!IsCompleted, "Cannot update build progress after building is completed.");
            buildProgress = newBuildProgress;
            HealthAdded?.Invoke(healthAdded);
        }

        public void SetBuildCompleted()
        {
            DebugAssert.State.Satisfies(!IsCompleted, "Cannot complete building more than once.");
            Completing?.Invoke();
            IsCompleted = true;
        }

        public bool CanApplyUpgrade(UpgradeBlueprint upgrade)
        {
            return upgrade.CanApplyTo(Components);
        }

        public void ApplyUpgrade(UpgradeBlueprint upgrade)
        {
            upgrade.ApplyTo(Components);
        }

        protected override void OnDelete()
        {
            OccupiedTiles.ForEach(tile =>
            {
                Game.Navigator.RemoveSink(tile);
            });

            base.OnDelete();
        }

        public override void Draw(GeometryManager geometries)
        {
            var alpha = IsCompleted ? 1 : (float)(buildProgress * 0.9);
            DrawTiles(geometries, drawColors[SelectionState] * alpha);
            DrawBuildingName(geometries, Color.Black);
            base.Draw(geometries);

            geometries.PointLight.Draw(Position.NumericValue.WithZ(3), 3 + 2 * alpha, Color.Orange * 0.2f);
        }
    }
}
