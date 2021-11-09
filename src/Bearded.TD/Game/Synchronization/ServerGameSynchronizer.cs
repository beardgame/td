using System;
using System.Collections.Generic;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands.Synchronization;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Synchronization
{
    sealed class ServerGameSynchronizer : IGameSynchronizer
    {
        private static readonly TimeSpan timeBetweenSyncs = 0.5.S();

        private readonly Dictionary<Type, object> synchronizers = new();
        private readonly ICommandDispatcher<GameInstance> commandDispatcher;
        private Instant nextSync = Instant.Zero;

        public ServerGameSynchronizer(ICommandDispatcher<GameInstance> commandDispatcher)
        {
            this.commandDispatcher = commandDispatcher;

            synchronizers.Add(
                typeof(ComponentGameObject), new Synchronizer<ComponentGameObject>(SyncBuildings.Command));
            synchronizers.Add(typeof(EnemyUnit), new Synchronizer<EnemyUnit>(SyncEnemies.Command));
        }

        public void RegisterSyncable<T>(T syncable)
            where T : IDeletable
        {
            getSynchronizer<T>().Register(syncable);
        }

        public void Synchronize(GameInstance game)
        {
            if (game.State.Time >= nextSync)
            {
                foreach (var (_, synchronizer) in synchronizers)
                {
                    ((ISynchronizer)synchronizer).SendBatch(commandDispatcher);
                }

                nextSync = game.State.Time + timeBetweenSyncs;
            }
        }

        private Synchronizer<T> getSynchronizer<T>()
            where T : IDeletable
        {
            return (Synchronizer<T>) synchronizers[typeof(T)];
        }

        private interface ISynchronizer
        {
            void SendBatch(ICommandDispatcher<GameInstance> commandDispatcher);
        }

        private sealed class Synchronizer<T> : ISynchronizer where T : IDeletable
        {
            private readonly Queue<T> queue = new();

            private readonly Func<IEnumerable<T>, ISerializableCommand<GameInstance>> commandCreator;
            private readonly int batchSize;

            public Synchronizer(Func<IEnumerable<T>, ISerializableCommand<GameInstance>> commandCreator, int batchSize = 100)
            {
                this.commandCreator = commandCreator;
                this.batchSize = batchSize;
            }

            public void Register(T syncable)
            {
                queue.Enqueue(syncable);
            }

            public void SendBatch(ICommandDispatcher<GameInstance> commandDispatcher)
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
