using Bearded.TD.Game.Synchronization;
using Bearded.TD.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Testing.GameStates;

sealed class NoOpSynchronizer : IGameSynchronizer
{
    public void RegisterSyncable<T>(T syncable) where T : IDeletable {}
    public void Synchronize(ITimeSource timeSource) {}
}
