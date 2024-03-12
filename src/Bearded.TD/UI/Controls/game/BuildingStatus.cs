using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatus : IDisposable
{
    public IReadonlyBinding<bool> ShowExpanded => showExpanded;
    public IReadonlyBinding<ImmutableArray<Status>> Statuses => statuses;
    public IReadonlyBinding<ImmutableArray<UpgradeSlot>> Upgrades => upgrades;

    private readonly IStatusTracker statusTracker;
    private readonly IUpgradeSlots upgradeSlots;
    private readonly IBuildingUpgradeManager upgradeManager;

    private readonly Binding<bool> showExpanded = new(false);
    private readonly Binding<ImmutableArray<Status>> statuses = new();
    private readonly Binding<ImmutableArray<UpgradeSlot>> upgrades = new();

    public BuildingStatus(
        IStatusTracker statusTracker,
        IUpgradeSlots upgradeSlots,
        IBuildingUpgradeManager upgradeManager)
    {
        this.statusTracker = statusTracker;
        this.upgradeSlots = upgradeSlots;
        this.upgradeManager = upgradeManager;

        statuses.SetFromSource(statusTracker.Statuses.ToImmutableArray());
        upgrades.SetFromSource(buildInitialUpgradeSlots());

        statusTracker.StatusAdded += statusAdded;
        statusTracker.StatusRemoved += statusRemoved;
        // TODO: handle new upgrades, new upgrade slots, and add the veterancy slot
    }

    private ImmutableArray<UpgradeSlot> buildInitialUpgradeSlots()
    {
        // TODO: there is some leftover tech debt from when we still allowed queueing upgrades.
        // Explicitly crash right now if we find ourselves in that old state.
        DebugAssert.State.Satisfies(upgradeSlots.ReservedSlotsCount == 0);
        var appliedUpgrades = upgradeManager.AppliedUpgrades.ToList();
        DebugAssert.State.Satisfies(appliedUpgrades.Count == upgradeSlots.FilledSlotsCount);

        return Enumerable.Range(0, upgradeSlots.TotalSlotsCount)
            .Select(i =>
            {
                if (i >= appliedUpgrades.Count)
                {
                    return new UpgradeSlot(null, null);
                }

                return new UpgradeSlot(appliedUpgrades[i], null);
            })
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

    public void PromoteToExpandedView()
    {
        showExpanded.SetFromSource(true);
    }

    public void Dispose()
    {
        statusTracker.StatusAdded -= statusAdded;
        statusTracker.StatusRemoved -= statusRemoved;
    }
}
