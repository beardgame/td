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
    private readonly List<IUpgradeBlueprint> appliedUpgrades = new();
    public IReadOnlyCollection<IUpgradeBlueprint> AppliedUpgrades { get; }
    private readonly Dictionary<ModAwareId, IncompleteUpgrade> upgradesInProgress = new();
    public IReadOnlyCollection<IIncompleteUpgrade> UpgradesInProgress =>
        upgradesInProgress.Values.ToImmutableArray();

    public int UpgradeSlotsUnlocked { get; private set; }
    public int UpgradeSlotsOccupied => appliedUpgrades.Count + upgradesInProgress.Count;

    public IEnumerable<IUpgradeBlueprint> ApplicableUpgrades
    {
        get
        {
            if (factionProvider == null)
            {
                return Enumerable.Empty<IUpgradeBlueprint>();
            }
            return factionProvider.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology)
                ? technology.GetApplicableUpgradesFor(this)
                : Enumerable.Empty<IUpgradeBlueprint>();
        }
    }

    public event GenericEventHandler<IIncompleteUpgrade>? UpgradeQueued;
    public event GenericEventHandler<IUpgradeBlueprint>? UpgradeCompleted;

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

    public bool CanApplyUpgrade(IUpgradeBlueprint upgrade)
    {
        return !appliedUpgrades.Contains(upgrade) && upgrade.CanApplyTo(Owner);
    }

    public IIncompleteUpgrade QueueUpgrade(IUpgradeBlueprint upgrade)
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

        incompleteUpgrade.Upgrade.ApplyTo(Owner);

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
    IReadOnlyCollection<IUpgradeBlueprint> AppliedUpgrades { get; }
    IReadOnlyCollection<IIncompleteUpgrade> UpgradesInProgress { get; }
    IEnumerable<IUpgradeBlueprint> ApplicableUpgrades { get; }
    bool HasAvailableSlot { get; }
    int UpgradeSlotsUnlocked { get; }
    int UpgradeSlotsOccupied { get; }

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
