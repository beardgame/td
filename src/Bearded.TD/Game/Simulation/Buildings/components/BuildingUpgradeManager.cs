using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Synchronization;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed partial class BuildingUpgradeManager<T>
        : Component<T>,
            IBuildingUpgradeManager,
            IBuildingUpgradeSyncer,
            IUpgradable
        where T : IComponentOwner<T>, IFactioned, IGameObject, INamed
    {
        private readonly List<IUpgradeBlueprint> appliedUpgrades = new();
        public IReadOnlyCollection<IUpgradeBlueprint> AppliedUpgrades { get; }
        private readonly Dictionary<ModAwareId, IncompleteUpgrade> upgradesInProgress = new();
        public IReadOnlyCollection<IIncompleteUpgrade> UpgradesInProgress =>
            upgradesInProgress.Values.ToImmutableArray();

        public IEnumerable<IUpgradeBlueprint> ApplicableUpgrades =>
            Owner.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology)
                ? technology.GetApplicableUpgradesFor(this)
                : Enumerable.Empty<IUpgradeBlueprint>();

        public event GenericEventHandler<IIncompleteUpgrade>? UpgradeQueued;
        public event GenericEventHandler<IUpgradeBlueprint>? UpgradeCompleted;

        public BuildingUpgradeManager()
        {
            AppliedUpgrades = appliedUpgrades.AsReadOnly();
        }

        protected override void Initialize()
        {
            Events.Send(new ReportAdded(new UpgradeReport(this)));
        }

        public bool CanBeUpgradedBy(Faction faction) => faction.SharesBehaviorWith<FactionResources>(Owner.Faction);

        public bool CanApplyUpgrade(IUpgradeBlueprint upgrade)
        {
            return !appliedUpgrades.Contains(upgrade) && upgrade.CanApplyTo(Owner);
        }

        public IIncompleteUpgrade QueueUpgrade(IUpgradeBlueprint upgrade)
        {
            var incompleteUpgrade = new IncompleteUpgrade(this, upgrade);
            upgradesInProgress.Add(upgrade.Id, incompleteUpgrade);

            UpgradeQueued?.Invoke(incompleteUpgrade);
            Owner.Game.Meta.Events.Send(new BuildingUpgradeQueued(Owner.Name, Owner, incompleteUpgrade));

            return incompleteUpgrade;
        }

        private void onUpgradeCompleted(IIncompleteUpgrade incompleteUpgrade)
        {
            if (!upgradesInProgress.Remove(incompleteUpgrade.Upgrade.Id))
            {
                State.IsInvalid();
                return;
            }

            incompleteUpgrade.Upgrade.ApplyTo(Owner);

            appliedUpgrades.Add(incompleteUpgrade.Upgrade);

            UpgradeCompleted?.Invoke(incompleteUpgrade.Upgrade);
            Owner.Game.Meta.Events.Send(new BuildingUpgradeFinished(Owner.Name, Owner, incompleteUpgrade.Upgrade));
        }

        private void onUpgradeCancelled(IIncompleteUpgrade incompleteUpgrade)
        {
            if (!upgradesInProgress.Remove(incompleteUpgrade.Upgrade.Id))
            {
                State.IsInvalid();
                return;
            }

            Owner.Game.Meta.Events.Send(new BuildingUpgradeCancelled(Owner.Name, Owner, incompleteUpgrade.Upgrade));
        }

        public void SyncStartUpgrade(ModAwareId upgradeId)
        {
            if (!upgradesInProgress.TryGetValue(upgradeId, out var incompleteUpgrade))
            {
                State.IsInvalid();
                return;
            }
            incompleteUpgrade.SyncStartUpgrade();
        }

        public void SyncCompleteUpgrade(ModAwareId upgradeId)
        {
            if (!upgradesInProgress.TryGetValue(upgradeId, out var incompleteUpgrade))
            {
                State.IsInvalid();
                return;
            }
            incompleteUpgrade.SyncCompleteUpgrade();
        }

        private void sendSyncUpgradeStart(IIncompleteUpgrade incompleteUpgrade)
        {
            // TODO: currently cast needed to get the building ID
            Owner.Sync(SyncUpgradeStart.Command, Owner as Building, incompleteUpgrade.Upgrade.Id);
        }

        private void sendSyncUpgradeCompletion(IIncompleteUpgrade incompleteUpgrade)
        {
            // TODO: currently cast needed to get the building ID
            Owner.Sync(SyncUpgradeCompletion.Command, Owner as Building, incompleteUpgrade.Upgrade.Id);
        }

        public override void Update(TimeSpan elapsedTime) {}
        public override void Draw(CoreDrawers drawers) {}
    }

    interface IBuildingUpgradeManager
    {
        IReadOnlyCollection<IUpgradeBlueprint> AppliedUpgrades { get; }
        IReadOnlyCollection<IIncompleteUpgrade> UpgradesInProgress { get; }
        IEnumerable<IUpgradeBlueprint> ApplicableUpgrades { get; }

        event GenericEventHandler<IIncompleteUpgrade> UpgradeQueued;
        event GenericEventHandler<IUpgradeBlueprint> UpgradeCompleted;

        bool CanApplyUpgrade(IUpgradeBlueprint upgrade);
        bool CanBeUpgradedBy(Faction faction);
        IIncompleteUpgrade QueueUpgrade(IUpgradeBlueprint upgrade);
    }

    interface IBuildingUpgradeSyncer
    {
        void SyncStartUpgrade(ModAwareId upgradeId);
        void SyncCompleteUpgrade(ModAwareId upgradeId);
    }
}
