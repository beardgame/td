using Bearded.TD.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Synchronization;

interface IGameSynchronizer
{
    void RegisterSyncable<T>(T syncable) where T : IDeletable;
    void Synchronize(ITimeSource timeSource);
}
