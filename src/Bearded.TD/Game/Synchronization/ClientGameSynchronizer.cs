using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Synchronization;

sealed class ClientGameSynchronizer : IGameSynchronizer
{
    public void RegisterSyncable<T>(T syncable) where T : IDeletable { }
    public void Synchronize(GameInstance game) { }
}