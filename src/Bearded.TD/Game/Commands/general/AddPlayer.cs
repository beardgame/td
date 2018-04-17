using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class AddPlayer
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, Player player)
            => new Implementation(game, player);

        private class Implementation : ISerializableCommand<GameInstance>
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

            public ICommandSerializer<GameInstance> Serializer => new Serializer(player);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Player> id;
            private string name;

            public Serializer(Player player)
            {
                id = player.Id;
                name = player.Name;
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game, new Player(id, name));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref id);
                stream.Serialize(ref name);
            }
        }
    }
}