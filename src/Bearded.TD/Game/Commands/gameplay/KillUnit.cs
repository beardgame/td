using Bearded.TD.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class KillUnit
    {
        public static ICommand Command(GameUnit unit, Faction faction) => new Implementation(unit, faction);

        private class Implementation : ICommand
        {
            private readonly GameUnit unit;
            private readonly Faction killingBlowFaction;

            public Implementation(GameUnit unit, Faction killingBlowFaction)
            {
                this.unit = unit;
                this.killingBlowFaction = killingBlowFaction;
            }

            public void Execute() => unit.Kill(killingBlowFaction);
            public ICommandSerializer Serializer => new Serializer(unit, killingBlowFaction);
        }

        private class Serializer : ICommandSerializer
        {
            private Id<GameUnit> unit;
            private Id<Faction> killingBlowFaction;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(GameUnit unit, Faction killingBlowFaction)
            {
                this.unit = unit.Id;
                this.killingBlowFaction = killingBlowFaction?.Id ?? Id<Faction>.Invalid;
            }

            public ICommand GetCommand(GameInstance game)
            {
                return new Implementation(game.State.Find(unit), killingBlowFaction.IsValid ? game.State.FactionFor(killingBlowFaction) : null);
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref unit);
                stream.Serialize(ref killingBlowFaction);
            }
        }
    }
}