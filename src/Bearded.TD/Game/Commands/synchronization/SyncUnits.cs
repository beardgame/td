using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SyncUnits
    {
        public static ICommand Command(IEnumerable<GameUnit> units)
            => new Implementation(units.Select(u => (u, u.GetCurrentState())).ToList());

        private class Implementation : ICommand
        {
            private readonly IList<(GameUnit, GameUnitState)> units;

            public Implementation(IList<(GameUnit, GameUnitState)> units)
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

            public ICommandSerializer Serializer => new Serializer(units);
        }

        private class Serializer : ICommandSerializer
        {
            private (Id<GameUnit> unit, GameUnitState state)[] units;

            public Serializer(IList<(GameUnit unit, GameUnitState state)> units)
            {
                this.units = units.Select(tuple => (tuple.unit.Id, tuple.state)).ToArray();
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ICommand GetCommand(GameInstance game) =>
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
