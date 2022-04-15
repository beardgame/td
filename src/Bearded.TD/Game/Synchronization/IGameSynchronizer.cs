using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Synchronization;

interface IGameSynchronizer
{
    void RegisterSyncable(GameObject syncable);
    void Synchronize(ITimeSource timeSource);
}
