using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatus : IDisposable
{
    public IReadonlyBinding<bool> ShowExpanded => showExpanded;
    public IReadonlyBinding<ImmutableArray<Status>> Statuses => statuses;
    public IReadonlyBinding<ImmutableArray<UpgradeSlot>> Upgrades => upgrades;
    public IReadonlyBinding<VeterancyStatus> Veterancy => veterancyStatus;

    private readonly IStatusTracker statusTracker;
    private readonly IUpgradeSlots upgradeSlots;
    private readonly IBuildingUpgradeManager upgradeManager;
    private readonly IVeterancy veterancy;

    private readonly Binding<bool> showExpanded = new(false);
    private readonly Binding<ImmutableArray<Status>> statuses = new();
    private readonly Binding<ImmutableArray<UpgradeSlot>> upgrades = new();
    private readonly Binding<VeterancyStatus> veterancyStatus = new();

    public BuildingStatus(
        IStatusTracker statusTracker,
        IUpgradeSlots upgradeSlots,
        IBuildingUpgradeManager upgradeManager,
        IVeterancy veterancy)
    {
        this.statusTracker = statusTracker;
        this.upgradeSlots = upgradeSlots;
        this.upgradeManager = upgradeManager;
        this.veterancy = veterancy;

        statuses.SetFromSource(statusTracker.Statuses.ToImmutableArray());
        upgrades.SetFromSource(buildInitialUpgradeSlots());
        veterancyStatus.SetFromSource(veterancy.GetVeterancyStatus());

        statusTracker.StatusAdded += statusAdded;
        statusTracker.StatusRemoved += statusRemoved;
        veterancy.VeterancyStatusChanged += veterancyStatusChanged;

        // TODO: handle new upgrades, new upgrade slots
    }

    private ImmutableArray<UpgradeSlot> buildInitialUpgradeSlots()
    {
        var appliedUpgrades = upgradeManager.AppliedUpgrades.ToList();
        DebugAssert.State.Satisfies(appliedUpgrades.Count == upgradeSlots.FilledSlotsCount);

        return Enumerable.Range(0, upgradeSlots.TotalSlotsCount)
            .Select(i =>
                i >= appliedUpgrades.Count ? new UpgradeSlot(null, null) : new UpgradeSlot(appliedUpgrades[i], null))
            .ToImmutableArray();
    }

    private void statusAdded(Status status)
    {
        statuses.SetFromSource(statuses.Value.Add(status));
    }

    private void statusRemoved(Status status)
    {
        statuses.SetFromSource(statuses.Value.Remove(status));
    }

    private void veterancyStatusChanged(VeterancyStatus status)
    {
        veterancyStatus.SetFromSource(status);
    }

    public void PromoteToExpandedView()
    {
        showExpanded.SetFromSource(true);
    }

    public void Dispose()
    {
        statusTracker.StatusAdded -= statusAdded;
        statusTracker.StatusRemoved -= statusRemoved;
        veterancy.VeterancyStatusChanged -= veterancyStatusChanged;
    }
}
