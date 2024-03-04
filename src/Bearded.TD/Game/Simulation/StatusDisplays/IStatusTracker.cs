using System.Collections.Generic;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

interface IStatusTracker
{
    IReadOnlyList<HitPointsBar> HitPointsBars { get; }
    IReadOnlyList<Status> Statuses { get; }

    void AddHitPointsBar(HitPointsBar bar);
    IStatusReceipt AddStatus(StatusSpec status, Instant? expiryTime);
}
