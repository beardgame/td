using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed partial class BuildingUpgradeManager
    : Component,
        IBuildingUpgradeManager,
        IBuildingUpgradeSyncer,
        IUpgradable,
        IListener<GainLevel>
{
    private INameProvider? nameProvider;
    private IFactionProvider? factionProvider;
    private readonly List<IPermanentUpgrade> appliedUpgrades = new();
    public IReadOnlyCollection<IPermanentUpgrade> AppliedUpgrades { get; }
    private readonly Dictionary<ModAwareId, IncompleteUpgrade> upgradesInProgress = new();
    public IReadOnlyCollection<IIncompleteUpgrade> UpgradesInProgress =>
        upgradesInProgress.Values.ToImmutableArray();

    public int UpgradeSlotsUnlocked { get; private set; }
    public int UpgradeSlotsOccupied => appliedUpgrades.Count + upgradesInProgress.Count;

    public IEnumerable<IPermanentUpgrade> ApplicableUpgrades
    {
        get
        {
            if (factionProvider == null)
            {
                return Enumerable.Empty<IPermanentUpgrade>();
            }
            return factionProvider.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology)
                ? technology.GetApplicableUpgradesFor(this)
                : Enumerable.Empty<IPermanentUpgrade>();
        }
    }

    public event GenericEventHandler<IIncompleteUpgrade>? UpgradeQueued;
    public event GenericEventHandler<IPermanentUpgrade>? UpgradeCompleted;

    public BuildingUpgradeManager()
    {
        AppliedUpgrades = appliedUpgrades.AsReadOnly();
    }

    protected override void OnAdded()
    {
        ReportAggregator.Register(Events, new UpgradeReport(this));
        ComponentDependencies.Depend<INameProvider>(Owner, Events, provider => nameProvider = provider);
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider => factionProvider = provider);
        Events.Subscribe(this);
    }

    public bool HasAvailableSlot => UpgradeSlotsOccupied < UpgradeSlotsUnlocked;

    public bool CanBeUpgradedBy(Faction faction) =>
        factionProvider?.Faction.OwnedBuildingsCanBeUpgradedBy(faction) ?? false;

    public bool CanApplyUpgrade(IPermanentUpgrade upgrade)
    {
        return !appliedUpgrades.Contains(upgrade) && Owner.CanApplyUpgrade(upgrade);
    }

    public IIncompleteUpgrade QueueUpgrade(IPermanentUpgrade upgrade)
    {
        var incompleteUpgrade = new IncompleteUpgrade(this, upgrade);
        upgradesInProgress.Add(upgrade.Id, incompleteUpgrade);

        UpgradeQueued?.Invoke(incompleteUpgrade);
        Owner.Game.Meta.Events.Send(
            new BuildingUpgradeQueued(nameProvider.NameOrDefault(), Owner, incompleteUpgrade));

        return incompleteUpgrade;
    }

    private void onUpgradeCompleted(IIncompleteUpgrade incompleteUpgrade)
    {
        if (!upgradesInProgress.Remove(incompleteUpgrade.Upgrade.Id))
        {
            State.IsInvalid();
            return;
        }

        Owner.ApplyUpgrade(incompleteUpgrade.Upgrade);

        appliedUpgrades.Add(incompleteUpgrade.Upgrade);

        UpgradeCompleted?.Invoke(incompleteUpgrade.Upgrade);
        Owner.Game.Meta.Events.Send(
            new BuildingUpgradeFinished(nameProvider.NameOrDefault(), Owner, incompleteUpgrade.Upgrade));
    }

    private void onUpgradeCancelled(IIncompleteUpgrade incompleteUpgrade)
    {
        if (!upgradesInProgress.Remove(incompleteUpgrade.Upgrade.Id))
        {
            State.IsInvalid();
            return;
        }

        Owner.Game.Meta.Events.Send(
            new BuildingUpgradeCancelled(nameProvider.NameOrDefault(), Owner, incompleteUpgrade.Upgrade));
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
        Owner.Sync(SyncUpgradeStart.Command, Owner, incompleteUpgrade.Upgrade.Id);
    }

    private void sendSyncUpgradeCompletion(IIncompleteUpgrade incompleteUpgrade)
    {
        Owner.Sync(SyncUpgradeCompletion.Command, Owner, incompleteUpgrade.Upgrade.Id);
    }

    public override void Update(TimeSpan elapsedTime) {}

    public void HandleEvent(GainLevel @event) => UpgradeSlotsUnlocked++;
}

interface IBuildingUpgradeManager
{
    IReadOnlyCollection<IPermanentUpgrade> AppliedUpgrades { get; }
    IReadOnlyCollection<IIncompleteUpgrade> UpgradesInProgress { get; }
    IEnumerable<IPermanentUpgrade> ApplicableUpgrades { get; }
    bool HasAvailableSlot { get; }
    int UpgradeSlotsUnlocked { get; }
    int UpgradeSlotsOccupied { get; }

    event GenericEventHandler<IIncompleteUpgrade> UpgradeQueued;
    event GenericEventHandler<IPermanentUpgrade> UpgradeCompleted;

    bool CanApplyUpgrade(IPermanentUpgrade upgrade);
    bool CanBeUpgradedBy(Faction faction);
    IIncompleteUpgrade QueueUpgrade(IPermanentUpgrade upgrade);
}

interface IBuildingUpgradeSyncer
{
    void SyncStartUpgrade(ModAwareId upgradeId);
    void SyncCompleteUpgrade(ModAwareId upgradeId);
}
