using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Synchronization;

sealed class ClientGameSynchronizer : IGameSynchronizer
{
    public void RegisterSyncable(GameObject syncable) { }
    public void Synchronize(ITimeSource timeSource) { }
}
