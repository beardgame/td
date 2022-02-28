using Bearded.TD.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Synchronization;

sealed class ClientGameSynchronizer : IGameSynchronizer
{
    public void RegisterSyncable<T>(T syncable) where T : IDeletable { }
    public void Synchronize(ITimeSource timeSource) { }
}
