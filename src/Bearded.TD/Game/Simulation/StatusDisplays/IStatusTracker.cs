using System.Collections.Generic;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

interface IStatusTracker
{
    IReadOnlyList<HitPointsBar> HitPointsBars { get; }
    IReadOnlyList<Status> Statuses { get; }

    event StatusEventHandler StatusAdded;
    event StatusEventHandler StatusRemoved;

    void AddHitPointsBar(HitPointsBar bar);
    IStatusReceipt AddStatus(StatusSpec status, Instant? expiryTime);
}
