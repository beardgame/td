using System;
using System.Collections.Generic;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.IO;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Synchronization
{
    class ServerGameSynchronizer : IGameSynchronizer
    {
        private static readonly TimeSpan timeBetweenSyncs = new TimeSpan(.5); 

        private readonly Dictionary<Type, object> synchronizers = new Dictionary<Type, object>();
        private readonly ICommandDispatcher commandDispatcher;
        private readonly Logger logger;
        private Instant nextSync = Instant.Zero;

        public ServerGameSynchronizer(ICommandDispatcher commandDispatcher, Logger logger)
        {
            this.commandDispatcher = commandDispatcher;
            this.logger = logger;

            synchronizers.Add(typeof(EnemyUnit), new Synchronizer<EnemyUnit>(SyncUnits.Command));
        }

        public void RegisterSyncable<T>(T syncable)
            where T : IDeletable
        {
            getSynchronizer<T>().Register(syncable);
        }

        public void Synchronize(Instant currentTimestamp)
        {
            if (currentTimestamp >= nextSync)
            {
                logger.Trace.Log("Starting sync round");
                foreach (var (_, synchronizer) in synchronizers)
                    ((ISynchronizer) synchronizer).SendBatch(commandDispatcher);
                nextSync = currentTimestamp + timeBetweenSyncs;
            }
        }

        private Synchronizer<T> getSynchronizer<T>()
            where T : IDeletable
        {
            return (Synchronizer<T>) synchronizers[typeof(T)];
        }

        private interface ISynchronizer
        {
            void SendBatch(ICommandDispatcher commandDispatcher);
        }

        private class Synchronizer<T> : ISynchronizer where T : IDeletable
        {
            private readonly Queue<T> queue = new Queue<T>();

            private readonly Func<IEnumerable<T>, ICommand> commandCreator;
            private readonly int batchSize;

            public Synchronizer(Func<IEnumerable<T>, ICommand> commandCreator, int batchSize = 100)
            {
                this.commandCreator = commandCreator;
                this.batchSize = batchSize;
            }

            public void Register(T syncable)
            {
                queue.Enqueue(syncable);
            }

            public void SendBatch(ICommandDispatcher commandDispatcher)
            {
                commandDispatcher.Dispatch(commandCreator(getBatch()));
            }

            private IEnumerable<T> getBatch()
            {
                var numToSend = Math.Min(batchSize, queue.Count);
                for (int i = 0; i < numToSend; i++)
                {
                    var obj = queue.Dequeue();
                    if (obj.Deleted) continue;
                    queue.Enqueue(obj);
                    yield return obj;
                }
            }
        }
    }
}
