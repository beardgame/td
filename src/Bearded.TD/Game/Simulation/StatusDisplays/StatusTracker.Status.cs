using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed partial class StatusTracker
{
    private readonly List<Status> statuses = [];
    public ReadOnlyCollection<Status> Statuses { get; }

    public event StatusEventHandler? StatusAdded;
    public event StatusEventHandler? StatusRemoved;

    public IStatusReceipt AddStatus(StatusSpec spec, StatusAppearance appearance, Instant? expiryTime)
    {
        var status = new Status(spec, appearance) { Expiry = expiryTime };
        statuses.Add(status);
        StatusAdded?.Invoke(status);
        return new StatusReceipt(status, this);
    }

    private void removeStatus(Status status)
    {
        var success = statuses.Remove(status);
        System.Diagnostics.Debug.Assert(success);
        StatusRemoved?.Invoke(status);
    }

    private sealed class StatusReceipt(Status status, StatusTracker tracker) : IStatusReceipt
    {
        public void UpdateAppearance(StatusAppearance appearance)
        {
            status.Appearance = appearance;
        }

        public void DeleteImmediately()
        {
            tracker.removeStatus(status);
        }

        public void SetExpiryTime(Instant expiryTime)
        {
            status.Expiry = expiryTime;
        }
    }
}
