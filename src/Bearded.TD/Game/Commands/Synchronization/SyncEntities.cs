using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Linq;
using Lidgren.Network;

namespace Bearded.TD.Game.Commands.Synchronization
{
    static class SyncEntities
    {
        public abstract class Implementation<T> : ISerializableCommand<GameInstance>
            where T : class, IIdable<T>, ISyncable
        {
            private readonly IList<(T, IStateToSync)> syncedObjects;

            protected Implementation(IList<(T, IStateToSync)> syncedObjects)
            {
                this.syncedObjects = syncedObjects;
            }

            public void Execute()
            {
                foreach (var (_, stateToSync) in syncedObjects)
                {
                    stateToSync.Apply();
                }
            }

            public ICommandSerializer<GameInstance> Serializer => ToSerializer(syncedObjects);

            protected abstract ICommandSerializer<GameInstance> ToSerializer(
                IEnumerable<(T, IStateToSync)> syncedObjects);
        }

        public abstract class Serializer<T> : ICommandSerializer<GameInstance> where T : class, IIdable<T>, ISyncable
        {
            private (Id<T> id, byte[] data)[] syncedObjects = System.Array.Empty<(Id<T> id, byte[] data)>();

            protected Serializer() {}

            protected Serializer(IEnumerable<(T obj, IStateToSync synchronizer)> syncedObjects)
            {
                this.syncedObjects = syncedObjects
                    .Select(tuple => (tuple.obj.Id, dataFromStateToSync(tuple.synchronizer)))
                    .ToArray();
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
                ToImplementation(syncedObjects.SelectMany(tuple =>
                {
                    var (id, data) = tuple;
                    if (!game.State.TryFind(id, out var unit))
                    {
                        game.Meta.Logger.Debug?.Log(
                            $"Trying to sync an object that doesn't exist. Type: {typeof(T)} ID: {id}");
                        return Enumerable.Empty<(T obj, IStateToSync synchronizer)>();
                    }
                    var synchronizer = populatedStateToSyncFor(unit, data);
                    return (unit, synchronizer).Yield();
                }).ToImmutableArray());

            protected abstract ISerializableCommand<GameInstance> ToImplementation(
                ImmutableArray<(T, IStateToSync)> syncedObjects);

            public void Serialize(INetBufferStream stream)
            {
                stream.SerializeArrayCount(ref syncedObjects);
                for (var i = 0; i < syncedObjects.Length; i++)
                {
                    stream.Serialize(ref syncedObjects[i].id);
                    stream.Serialize(ref syncedObjects[i].data);
                }
            }

            private static byte[] dataFromStateToSync(IStateToSync data)
            {
                var buffer = new NetBuffer();
                var writer = new NetBufferWriter(buffer);
                data.Serialize(writer);
                return buffer.Data;
            }

            private static IStateToSync populatedStateToSyncFor(ISyncable unit, byte[] data)
            {
                var stateToSync = unit.GetCurrentStateToSync();
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
}
