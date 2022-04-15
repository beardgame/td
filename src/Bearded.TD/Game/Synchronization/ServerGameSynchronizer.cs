using System;
using System.Collections.Generic;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Synchronization;
using Bearded.TD.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Synchronization;

sealed class ServerGameSynchronizer : IGameSynchronizer
{
    private static readonly TimeSpan timeBetweenSyncs = 0.5.S();

    private readonly Synchronizer<GameObject> synchronizer = new(SyncGameObjects.Command);
    private readonly ICommandDispatcher<GameInstance> commandDispatcher;
    private Instant nextSync = Instant.Zero;

    public ServerGameSynchronizer(ICommandDispatcher<GameInstance> commandDispatcher)
    {
        this.commandDispatcher = commandDispatcher;
    }

    public void RegisterSyncable(GameObject gameObject)
    {
        synchronizer.Register(gameObject);
    }

    public void Synchronize(ITimeSource timeSource)
    {
        if (timeSource.Time >= nextSync)
        {
            synchronizer.SendBatch(commandDispatcher);
            nextSync = timeSource.Time + timeBetweenSyncs;
        }
    }

    private sealed class Synchronizer<T> where T : IDeletable
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
            for (var i = 0; i < numToSend; i++)
            {
                var obj = queue.Dequeue();
                if (obj.Deleted) continue;
                queue.Enqueue(obj);
                yield return obj;
            }
        }
    }
}
