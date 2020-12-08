using Bearded.TD.Commands;
using Bearded.TD.Game.GameState.Factions;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class ClearTechnologyQueue
    {
        public static IRequest<Player, GameInstance> Request(Faction faction) => new Implementation(faction);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly Faction faction;

            public Implementation(Faction faction)
            {
                this.faction = faction;
            }

            public override bool CheckPreconditions(Player actor) => faction.SharesTechnologyWith(actor.Faction);

            public override void Execute()
            {
                faction.Technology.ClearTechnologyQueue();
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Faction> faction;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Faction faction)
            {
                this.faction = faction.Id;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game) =>
                new Implementation(game.State.FactionFor(faction));

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
            }
        }
    }
}
