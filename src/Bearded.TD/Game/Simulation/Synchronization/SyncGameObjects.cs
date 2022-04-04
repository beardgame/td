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
using JetBrains.Annotations;
using Lidgren.Network;

namespace Bearded.TD.Game.Simulation.Synchronization;

static class SyncGameObjects
{
    public static ISerializableCommand<GameInstance> Command(IEnumerable<GameObject> gameObjects)
        => new Implementation(gameObjects
            .Select(b => b.GetComponents<ISyncer>().Single())
            .Select(syncer => (syncer, syncer.GetCurrentStateToSync()))
            .ToImmutableArray());

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly IList<(ISyncer, IStateToSync)> syncers;

        public Implementation(IList<(ISyncer, IStateToSync)> syncers)
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

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(syncers);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private (Id<GameObject> id, byte[] data)[] syncers = System.Array.Empty<(Id<GameObject>, byte[])>();

        [UsedImplicitly]
        public Serializer() {}

        public Serializer(IEnumerable<(ISyncer syncer, IStateToSync state)> syncers)
        {
            this.syncers = syncers
                .Select(tuple => (EntityId: tuple.syncer.GameObjectId, dataFromStateToSync(tuple.state)))
                .ToArray();
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
            new Implementation(syncers.SelectMany(tuple =>
            {
                var (id, data) = tuple;
                if (!game.State.TryFind(id, out var entity))
                {
                    game.Meta.Logger.Debug?.Log(
                        $"Trying to sync a game object that doesn't exist. ID: {id}");
                    return Enumerable.Empty<(ISyncer, IStateToSync)>();
                }

                var syncer = entity.GetComponents<ISyncer>().SingleOrDefault();
                if (syncer == null)
                {
                    game.Meta.Logger.Debug?.Log(
                        $"Trying to sync a game object without syncer. ID: {id}");
                    return Enumerable.Empty<(ISyncer, IStateToSync)>();
                }

                var stateToSync = populatedStateToSyncFor(syncer, data);
                return (syncer, synchronizer: stateToSync).Yield();
            }).ToImmutableArray());

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

        private static IStateToSync populatedStateToSyncFor(ISyncer syncer, byte[] data)
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
