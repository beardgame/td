using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.GameState.Factions;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SetPlayerFaction
    {
        public static ISerializableCommand<GameInstance> Command(Player player, Faction faction)
            => new Implementation(player, faction);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly Player player;
            private readonly Faction faction;

            public Implementation(Player player, Faction faction)
            {
                this.player = player;
                this.faction = faction;
            }

            public void Execute()
            {
                player.SetFaction(faction);
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(player, faction);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Player> player;
            private Id<Faction> faction;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Player player, Faction faction)
            {
                this.player = player.Id;
                this.faction = faction.Id;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game.PlayerFor(player), game.State.FactionFor(faction));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref player);
                stream.Serialize(ref faction);
            }
        }
    }
}
