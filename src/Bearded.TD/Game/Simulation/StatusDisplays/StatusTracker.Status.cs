using System.Collections.Generic;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed partial class StatusTracker
{
    private readonly List<Status> statuses = [];
    public IReadOnlyList<Status> Statuses { get; }

    public IStatusReceipt AddStatus(StatusSpec spec, Instant? expiryTime)
    {
        var status = new Status(spec, expiryTime);
        statuses.Add(status);
        return new StatusReceipt(status, this);
    }

    private sealed class StatusReceipt(Status status, StatusTracker tracker) : IStatusReceipt
    {
        public void DeleteImmediately()
        {
            tracker.statuses.Remove(status);
        }

        public void SetExpiryTime(Instant expiryTime)
        {
            status.Expiry = expiryTime;
        }
    }
}
