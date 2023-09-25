using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

interface IStatusDisplay
{
    void AddHitPointsBar(HitPointsBar bar);
    IStatusReceipt AddStatus(StatusSpec status, Instant? expiryTime);
}
