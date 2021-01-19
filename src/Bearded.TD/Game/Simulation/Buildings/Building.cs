using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    sealed class Building : PlacedBuildingBase<Building>, IIdable<Building>, IMortal, IDamageOwner
    {
        private readonly List<BuildingUpgradeTask> upgradesInProgress = new();
        public ReadOnlyCollection<BuildingUpgradeTask> UpgradesInProgress { get; }
        private readonly List<IUpgradeBlueprint> appliedUpgrades = new();
        public ReadOnlyCollection<IUpgradeBlueprint> AppliedUpgrades { get; }

        public Id<Building> Id { get; }

        private readonly DamageExecutor damageExecutor;

        public bool IsCompleted { get; private set; }
        private bool isDead;

        public event VoidEventHandler? Completing;

        public Building(Id<Building> id, IBuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
            Id = id;
            AppliedUpgrades = appliedUpgrades.AsReadOnly();
            UpgradesInProgress = upgradesInProgress.AsReadOnly();
            damageExecutor = new DamageExecutor(Events);
        }

        protected override IEnumerable<IComponent<Building>> InitializeComponents()
            => Blueprint.GetComponentsForBuilding();

        public void AttributeDamage(IMortal target, DamageResult damageResult)
        {
            Events.Send(new CausedDamage(target, damageResult));
        }

        public DamageResult Damage(DamageInfo damage)
        {
            return damageExecutor.Damage(damage);
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
            Events.Send(new HealDamage(healthAdded));
        }

        public void SetBuildCompleted()
        {
            DebugAssert.State.Satisfies(!IsCompleted, "Cannot complete building more than once.");
            Completing?.Invoke();
            IsCompleted = true;
            Game.Meta.Events.Send(new BuildingConstructionFinished(this));
        }

        public bool CanBeUpgradedBy(Faction faction) => faction.SharesResourcesWith(Faction);

        public bool CanApplyUpgrade(IUpgradeBlueprint upgrade)
        {
            return !appliedUpgrades.Contains(upgrade) && upgrade.CanApplyTo(Components);
        }

        public void ApplyUpgrade(IUpgradeBlueprint upgrade)
        {
            upgrade.ApplyTo(Components);

            appliedUpgrades.Add(upgrade);
            Game.Meta.Events.Send(new BuildingUpgradeFinished(this, upgrade));
        }

        public void RegisterBuildingUpgradeTask(BuildingUpgradeTask task)
        {
            DebugAssert.Argument.Satisfies(task.Building == this, "Can only add tasks upgrading this building.");
            DebugAssert.Argument.Satisfies(
                upgradesInProgress.All(t => t.Upgrade != task.Upgrade),
                "Cannot queue an upgrade task for the same upgrade twice.");
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

        public IEnumerable<IUpgradeBlueprint> GetApplicableUpgrades() =>
            Faction.Technology?.GetApplicableUpgradesFor(this) ?? Enumerable.Empty<IUpgradeBlueprint>();
    }
}
