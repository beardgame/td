using Bearded.TD.Commands;
using Bearded.TD.Game.GameState.Factions;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands.Debug
{
    static class GrantTechPoints
    {
        public static IRequest<Player, GameInstance> Request(Faction faction, long number)
            => new Implementation(faction, number);

        private class Implementation : UnifiedDebugRequestCommand
        {
            private readonly Faction faction;
            private readonly long number;

            public Implementation(Faction faction, long number)
            {
                this.faction = faction;
                this.number = number;
            }

            protected override bool CheckPreconditionsDebug(Player _) => faction.HasResources;

            public override void Execute() => faction.Technology.AddTechPoints(number);

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, number);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Faction> faction;
            private long number;

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public Serializer(Faction faction, long number)
            {
                this.faction = faction.Id;
                this.number = number;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
                => new Implementation(game.State.FactionFor(faction), number);

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
                stream.Serialize(ref number);
            }
        }
    }
}
