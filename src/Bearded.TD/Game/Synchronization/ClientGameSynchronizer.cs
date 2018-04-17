using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Synchronization
{
    class ClientGameSynchronizer : IGameSynchronizer
    {
        public void RegisterSyncable<T>(T syncable) where T : IDeletable { }
        public void Synchronize(GameInstance game) { }
    }
}
