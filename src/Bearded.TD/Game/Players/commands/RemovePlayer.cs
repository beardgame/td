using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Players;

static class RemovePlayer
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
            game.RemovePlayer(player);
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(player);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<Player> player;

        public Serializer(Player player)
        {
            this.player = player.Id;
        }

        [UsedImplicitly]
        public Serializer()
        {
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game, game.PlayerFor(player));

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref player);
        }
    }
}