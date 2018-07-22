using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.World;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Buildings
{
    [ComponentOwner]
    class Building : PlacedBuildingBase<Building>, IIdable<Building>, IDamageable
    {
        private static readonly Dictionary<SelectionState, Color> drawColors = new Dictionary<SelectionState, Color>
        {
            {SelectionState.Default, Color.Blue},
            {SelectionState.Focused, Color.DarkBlue},
            {SelectionState.Selected, Color.RoyalBlue}
        };

        public Id<Building> Id { get; }

        private int health;
        public override int Health => health;
        public bool IsCompleted { get; private set; }
        private double buildProgress;

        public event VoidEventHandler Completing;
        public event VoidEventHandler Damaged;

        public Building(Id<Building> id, IBuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
            Id = id;
            health = 1;
        }

        protected override IEnumerable<IComponent<Building>> InitialiseComponents()
            => Blueprint.GetComponentsForBuilding();

        public void Damage(int damage)
        {
            if (!UserSettings.Instance.Debug.InvulnerableBuildings)
                health -= damage;
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

        public void SetBuildProgress(double newBuildProgress, int healthAdded)
        {
            DebugAssert.State.Satisfies(!IsCompleted, "Cannot update build progress after building is completed.");
            buildProgress = newBuildProgress;
            health += healthAdded;
        }

        public void SetBuildCompleted()
        {
            DebugAssert.State.Satisfies(!IsCompleted, "Cannot complete building more than once.");
            Completing?.Invoke();
            IsCompleted = true;
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
            var alpha = IsCompleted ? 1 : (float)(buildProgress * 0.9);
            DrawTiles(geometries, drawColors[SelectionState] * alpha);
            DrawBuildingName(geometries, Color.Black);
            base.Draw(geometries);

            geometries.PointLight.Draw(Position.NumericValue.WithZ(3), 3 + 2 * alpha, Color.Orange * 0.2f);
        }
    }
}
