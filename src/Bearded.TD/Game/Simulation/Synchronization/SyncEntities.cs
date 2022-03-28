using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Lidgren.Network;

namespace Bearded.TD.Game.Simulation.Synchronization;

static class SyncEntities
{
    public abstract class Implementation<T> : ISerializableCommand<GameInstance>
        where T : class, IComponentOwner
    {
        private readonly IList<(ISyncer<T>, IStateToSync)> syncers;

        protected Implementation(IList<(ISyncer<T>, IStateToSync)> syncers)
        {
            this.syncers = syncers;
        }

        public void Execute()
        {
            foreach (var (_, stateToSync) in syncers)
            {
                stateToSync.Apply();
            }
        }

        public ICommandSerializer<GameInstance> Serializer => ToSerializer(syncers);

        protected abstract ICommandSerializer<GameInstance> ToSerializer(
            IEnumerable<(ISyncer<T>, IStateToSync)> syncedObjects);
    }

    public abstract class Serializer<T> : ICommandSerializer<GameInstance>
        where T : class, IComponentOwner
    {
        private (Id<T> id, byte[] data)[] syncers = System.Array.Empty<(Id<T>, byte[])>();

        protected Serializer() {}

        protected Serializer(IEnumerable<(ISyncer<T> syncer, IStateToSync state)> syncers)
        {
            this.syncers = syncers
                .Select(tuple => (tuple.syncer.EntityId, dataFromStateToSync(tuple.state)))
                .ToArray();
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
            ToImplementation(syncers.SelectMany(tuple =>
            {
                var (id, data) = tuple;
                if (!game.State.TryFind(id, out var entity))
                {
                    game.Meta.Logger.Debug?.Log(
                        $"Trying to sync an object that doesn't exist. Type: {typeof(T)} ID: {id}");
                    return Enumerable.Empty<(ISyncer<T>, IStateToSync)>();
                }

                var syncer = entity.GetComponents<ISyncer<T>>().SingleOrDefault();
                if (syncer == null)
                {
                    game.Meta.Logger.Debug?.Log(
                        $"Trying to sync an object without syncer. Type: {typeof(T)} ID: {id}");
                    return Enumerable.Empty<(ISyncer<T>, IStateToSync)>();
                }

                var stateToSync = populatedStateToSyncFor(syncer, data);
                return (syncer, synchronizer: stateToSync).Yield();
            }).ToImmutableArray());

        protected abstract ISerializableCommand<GameInstance> ToImplementation(
            ImmutableArray<(ISyncer<T>, IStateToSync)> syncers);

        public void Serialize(INetBufferStream stream)
        {
            stream.SerializeArrayCount(ref syncers);
            for (var i = 0; i < syncers.Length; i++)
            {
                stream.Serialize(ref syncers[i].id);
                stream.Serialize(ref syncers[i].data);
            }
        }

        private static byte[] dataFromStateToSync(IStateToSync data)
        {
            var buffer = new NetBuffer();
            var writer = new NetBufferWriter(buffer);
            data.Serialize(writer);
            return buffer.Data;
        }

        private static IStateToSync populatedStateToSyncFor(ISyncer<T> syncer, byte[] data)
        {
            var stateToSync = syncer.GetCurrentStateToSync();
            populateStateToSyncFromData(stateToSync, data);
            return stateToSync;
        }

        private static void populateStateToSyncFromData(IStateToSync stateToSync, byte[] data)
        {
            var buffer = new NetBuffer();
            buffer.Write(data);
            var reader = new NetBufferReader(buffer);
            stateToSync.Serialize(reader);
        }
    }
}
