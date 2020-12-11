using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using JetBrains.Annotations;
using Lidgren.Network;

namespace Bearded.TD.Game.Commands.Synchronization
{
    static class SyncUnits
    {
        public static ISerializableCommand<GameInstance> Command(IEnumerable<EnemyUnit> units)
            => new Implementation(units.Select(u => (u, u.GetCurrentStateToSync())).ToList());

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly IList<(EnemyUnit, IStateToSync)> units;

            public Implementation(IList<(EnemyUnit, IStateToSync)> units)
            {
                this.units = units;
            }

            public void Execute()
            {
                foreach (var (_, stateToSync) in units)
                {
                    stateToSync.Apply();
                }
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(units);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private (Id<EnemyUnit> unit, byte[] data)[] units = {};

            [UsedImplicitly]
            public Serializer() {}

            public Serializer(IEnumerable<(EnemyUnit unit, IStateToSync synchronizer)> units)
            {
                this.units = units.Select(tuple => (tuple.unit.Id, dataFromStateToSync(tuple.synchronizer))).ToArray();
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
                new Implementation(units.SelectMany(tuple =>
                {
                    var (id, data) = tuple;
                    if (!game.State.TryFind(id, out var unit))
                    {
                        game.Meta.Logger.Debug?.Log($"Trying to sync an enemy that doesn't exist. ID: {id}");
                        return Enumerable.Empty<(EnemyUnit unit, IStateToSync synchronizer)>();
                    }
                    var synchronizer = populatedStateToSyncFor(unit, data);
                    return (unit, synchronizer).Yield();
                }).ToList());

            public void Serialize(INetBufferStream stream)
            {
                stream.SerializeArrayCount(ref units);
                for (var i = 0; i < units.Length; i++)
                {
                    stream.Serialize(ref units[i].unit);
                    stream.Serialize(ref units[i].data);
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
