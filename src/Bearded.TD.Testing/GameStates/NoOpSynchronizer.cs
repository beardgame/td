using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Testing.GameStates;

sealed class NoOpSynchronizer : IGameSynchronizer
{
    public void RegisterSyncable(GameObject syncable) {}
    public void Synchronize(ITimeSource timeSource) {}
}
