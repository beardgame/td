using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

interface IStatusReceipt
{
    void DeleteImmediately();
    void SetExpiryTime(Instant expiryTime);
}
