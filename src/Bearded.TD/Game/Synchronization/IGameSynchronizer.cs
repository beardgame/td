using System;
using Bearded.TD.Commands;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Synchronization
{
    interface IGameSynchronizer
    {
        void RegisterSyncable<T>(T syncable) where T : IDeletable;
        void RegisterSyncCommand(Func<ICommand> syncCommand);
        void Synchronize(Instant currentTimestamp);
    }
}
