using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SyncUnits
    {
        public static ISerializableCommand<GameInstance> Command(IEnumerable<EnemyUnit> units)
            => new Implementation(units.Select(u => (u, u.GetCurrentState())).ToList());

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly IList<(EnemyUnit, EnemyUnitState)> units;

            public Implementation(IList<(EnemyUnit, EnemyUnitState)> units)
            {
                this.units = units;
            }

            public void Execute()
            {
                foreach (var (unit, state) in units)
                {
                    unit.SyncFrom(state);
                }
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(units);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private (Id<EnemyUnit> unit, EnemyUnitState state)[] units;

            public Serializer(IList<(EnemyUnit unit, EnemyUnitState state)> units)
            {
                this.units = units.Select(tuple => (tuple.unit.Id, tuple.state)).ToArray();
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
                new Implementation(units.Select(tuple => (game.State.Find(tuple.unit), tuple.state)).ToList());

            public void Serialize(INetBufferStream stream)
            {
                stream.SerializeArrayCount(ref units);
                for (var i = 0; i < units.Length; i++)
                {
                    stream.Serialize(ref units[i].unit);
                    stream.Serialize(ref units[i].state);
                }
            }
        }
    }
}
