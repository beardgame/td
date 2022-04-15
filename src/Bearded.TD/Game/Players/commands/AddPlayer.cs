using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Players;

static class AddPlayer
{
    public static ISerializableCommand<GameInstance> Command(GameInstance game, Player player)
        => new Implementation(game, player);

    private sealed class Implementation : ISerializableCommand<GameInstance>
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

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(player);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<Player> id;
        private string name = "";

        [UsedImplicitly]
        public Serializer() {}

        public Serializer(Player player)
        {
            id = player.Id;
            name = player.Name;
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
