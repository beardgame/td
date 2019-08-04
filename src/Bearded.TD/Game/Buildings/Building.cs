using System.Collections.Generic;
using System.Collections.ObjectModel;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
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
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings
{
    [ComponentOwner]
    class Building : PlacedBuildingBase<Building>, IIdable<Building>, IMortal
    {
        private static readonly Dictionary<SelectionState, Color> drawColors = new Dictionary<SelectionState, Color>
        {
            {SelectionState.Default, Color.Blue},
            {SelectionState.Focused, Color.DarkBlue},
            {SelectionState.Selected, Color.RoyalBlue}
        };

        private readonly List<BuildingUpgradeTask> upgradesInProgress = new List<BuildingUpgradeTask>();
        public ReadOnlyCollection<BuildingUpgradeTask> UpgradesInProgress { get; }
        private readonly List<IUpgradeBlueprint> appliedUpgrades = new List<IUpgradeBlueprint>();
        public ReadOnlyCollection<IUpgradeBlueprint> AppliedUpgrades { get; }

        public Id<Building> Id { get; }

        public bool IsCompleted { get; private set; }
        private double buildProgress;
        private bool isDead;

        public event VoidEventHandler Completing;
        public event GenericEventHandler<int> Damaged;
        public event GenericEventHandler<int> Healed;

        public Building(Id<Building> id, IBuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
            Id = id;
            AppliedUpgrades = appliedUpgrades.AsReadOnly();
            UpgradesInProgress = upgradesInProgress.AsReadOnly();
        }

        protected override IEnumerable<IComponent<Building>> InitialiseComponents()
            => Blueprint.GetComponentsForBuilding();

        public void Damage(int damage)
        {
            Damaged?.Invoke(damage);
        }

        public void OnDeath()
        {
            isDead = true;
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
            Healed?.Invoke(healthAdded);
        }

        public void SetBuildCompleted()
        {
            DebugAssert.State.Satisfies(!IsCompleted, "Cannot complete building more than once.");
            Completing?.Invoke();
            IsCompleted = true;
        }

        public bool CanBeUpgradedBy(Faction faction) => faction.SharesWorkersWith(Faction);

        public bool CanApplyUpgrade(IUpgradeBlueprint upgrade)
        {
            return upgrade.CanApplyTo(Components);
        }

        public void ApplyUpgrade(IUpgradeBlueprint upgrade)
        {
            upgrade.ApplyTo(Components);

            appliedUpgrades.Add(upgrade);
        }

        public void RegisterBuildingUpgradeTask(BuildingUpgradeTask task)
        {
            DebugAssert.Argument.Satisfies(task.Building == this, "Can only add tasks upgrading this building.");
            DebugAssert.Argument.Satisfies(!upgradesInProgress.Contains(task), "Can not add same task more than once.");
            upgradesInProgress.Add(task);
        }

        public void UnregisterBuildingUpgradeTask(BuildingUpgradeTask task)
        {
            var wasRemoved = upgradesInProgress.Remove(task);

            DebugAssert.Argument.Satisfies(wasRemoved, "Can only remove task that was added previously.");
        }

        protected override void OnDelete()
        {
            OccupiedTiles.ForEach(tile =>
            {
                Game.Navigator.RemoveSink(tile);
            });

            base.OnDelete();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            if (isDead)
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

            geometries.PointLight.Draw(Position.NumericValue.WithZ(3), 3 + 2 * alpha, Color.Coral * 0.75f);
        }

        public IEnumerable<IUpgradeBlueprint> GetApplicableUpgrades() =>
            Faction.Technology.GetApplicableUpgradesFor(this);
    }
}
