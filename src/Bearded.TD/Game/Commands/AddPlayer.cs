using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class AddPlayer
    {
        public static ICommand Command(GameInstance game, Player player)
            => new Implementation(game, player);

        private class Implementation : ICommand
        {
            private readonly GameInstance game;
            private readonly Player player;

            public Implementation(GameInstance game, Player player)
            {
                this.game = game;
                this.player = player;
            }

            public void Execute()
            {
                game.AddPlayer(player);
            }

            public ICommandSerializer Serializer => new Serializer(player);
        }

        private class Serializer : ICommandSerializer
        {
            private Id<Player> id;
            private string name;
            private Color color;

            public Serializer(Player player)
            {
                id = player.Id;
                name = player.Name;
                color = player.Color;
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game, new Player(id, name, color));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref id);
                stream.Serialize(ref name);
                stream.Serialize(ref color);
            }
        }
    }
}