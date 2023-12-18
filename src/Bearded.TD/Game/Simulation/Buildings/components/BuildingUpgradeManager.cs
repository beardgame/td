using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
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
        IListener<ComponentAdded>
{
    private INameProvider? nameProvider;
    private IFactionProvider? factionProvider;
    private IUpgradeSlots? upgradeSlots;
    private readonly List<IPermanentUpgrade> appliedUpgrades = new();
    public IReadOnlyCollection<IPermanentUpgrade> AppliedUpgrades { get; }
    private readonly Dictionary<ModAwareId, IncompleteUpgrade> upgradesInProgress = new();
    public IReadOnlyCollection<IIncompleteUpgrade> UpgradesInProgress =>
        upgradesInProgress.Values.ToImmutableArray();

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
        Events.Subscribe(this);
        ReportAggregator.Register(Events, new UpgradeReport(this));
        ComponentDependencies.Depend<INameProvider>(Owner, Events, provider => nameProvider = provider);
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider => factionProvider = provider);
        ComponentDependencies.Depend<IUpgradeSlots>(Owner, Events, slots => upgradeSlots = slots);
    }

    public bool CanBeUpgradedBy(Faction faction) =>
        factionProvider?.Faction.OwnedBuildingsCanBeUpgradedBy(faction) ?? false;

    public bool CanApplyUpgrade(IPermanentUpgrade upgrade)
    {
        return !appliedUpgrades.Contains(upgrade) && Owner.CanApplyUpgrade(upgrade);
    }

    public void Upgrade(IPermanentUpgrade upgrade)
    {
        if (upgradeSlots == null)
        {
            throw new InvalidOperationException("Cannot queue upgrade if there are no upgrade slots.");
        }
        applyUpgrade(upgrade);
        upgradeSlots.ReserveSlot().Fill();
    }

    public IIncompleteUpgrade QueueUpgrade(IPermanentUpgrade upgrade)
    {
        if (upgradeSlots == null)
        {
            throw new InvalidOperationException("Cannot queue upgrade if there are no upgrade slots.");
        }
        var incompleteUpgrade = new IncompleteUpgrade(this, upgrade, upgradeSlots.ReserveSlot());
        upgradesInProgress.Add(upgrade.Id, incompleteUpgrade);

        UpgradeQueued?.Invoke(incompleteUpgrade);

        return incompleteUpgrade;
    }

    private void onUpgradeCompleted(IIncompleteUpgrade incompleteUpgrade)
    {
        if (!upgradesInProgress.Remove(incompleteUpgrade.Upgrade.Id))
        {
            State.IsInvalid();
            return;
        }

        applyUpgrade(incompleteUpgrade.Upgrade);
    }

    private void applyUpgrade(IPermanentUpgrade upgrade)
    {
        Owner.ApplyUpgrade(upgrade);

        appliedUpgrades.Add(upgrade);

        UpgradeCompleted?.Invoke(upgrade);
        Owner.Game.Meta.Events.Send(
            new BuildingUpgradeFinished(nameProvider.NameOrDefault(), Owner, upgrade));
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

    public void HandleEvent(ComponentAdded @event)
    {
        // Replay all past upgrades on this new component
        foreach (var u in appliedUpgrades)
        {
            @event.Component.ApplyUpgrade(u);
        }
    }
}

interface IBuildingUpgradeManager
{
    IReadOnlyCollection<IPermanentUpgrade> AppliedUpgrades { get; }
    IReadOnlyCollection<IIncompleteUpgrade> UpgradesInProgress { get; }
    IEnumerable<IPermanentUpgrade> ApplicableUpgrades { get; }

    event GenericEventHandler<IIncompleteUpgrade> UpgradeQueued;
    event GenericEventHandler<IPermanentUpgrade> UpgradeCompleted;

    bool CanApplyUpgrade(IPermanentUpgrade upgrade);
    bool CanBeUpgradedBy(Faction faction);
    void Upgrade(IPermanentUpgrade upgrade);

    [Obsolete("Queueing of upgrades is no longer expected behaviour")]
    IIncompleteUpgrade QueueUpgrade(IPermanentUpgrade upgrade);
}

interface IBuildingUpgradeSyncer
{
    void SyncStartUpgrade(ModAwareId upgradeId);
    void SyncCompleteUpgrade(ModAwareId upgradeId);
}
