using System.Collections.ObjectModel;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

interface IStatusTracker
{
    ReadOnlyCollection<HitPointsBar> HitPointsBars { get; }
    ReadOnlyCollection<Status> Statuses { get; }

    event StatusEventHandler StatusAdded;
    event StatusEventHandler StatusRemoved;

    void AddHitPointsBar(HitPointsBar bar);
    IStatusReceipt AddStatus(StatusSpec status, Instant? expiryTime);
}
