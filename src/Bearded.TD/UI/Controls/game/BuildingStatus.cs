using System;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatus : IDisposable
{
    public IReadonlyBinding<bool> ShowExpanded => showExpanded;

    private readonly IStatusTracker statusTracker;

    private readonly Binding<bool> showExpanded = new(false);
    private readonly Binding<ImmutableArray<Status>> statuses = new();

    public BuildingStatus(IStatusTracker statusTracker)
    {
        this.statusTracker = statusTracker;

        statuses.SetFromSource(statusTracker.Statuses.ToImmutableArray());
        statusTracker.StatusAdded += statusAdded;
        statusTracker.StatusRemoved += statusRemoved;
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
