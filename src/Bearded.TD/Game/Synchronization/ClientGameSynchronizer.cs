using System;
using Bearded.TD.Commands;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Synchronization
{
    class ClientGameSynchronizer : IGameSynchronizer
    {
        public void RegisterSyncable<T>(T syncable) where T : IDeletable { }
        public void RegisterSyncCommand(Func<ICommand> syncCommand) { }
        public void Synchronize(Instant currentTimestamp) { }
    }
}
