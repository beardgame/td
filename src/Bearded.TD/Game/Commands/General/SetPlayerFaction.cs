using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SetPlayerFaction
    {
        public static ICommand Command(GameInstance game, Player player, Faction faction)
            => new Implementation(game, player, faction);

        private class Implementation : ICommand
        {
            private readonly GameInstance game;
            private readonly Player player;
            private readonly Faction faction;

            public Implementation(GameInstance game, Player player, Faction faction)
            {
                this.game = game;
                this.player = player;
                this.faction = faction;
            }

            public void Execute()
            {
                player.SetFaction(faction);
            }

            public ICommandSerializer Serializer => new Serializer(player, faction);
        }

        private class Serializer : ICommandSerializer
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

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game, game.PlayerFor(player), game.FactionFor(faction));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref player);
                stream.Serialize(ref faction);
            }
        }
    }
}
