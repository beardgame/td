using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Meta;

static class SendChatMessage
{
    public static IRequest<Player, GameInstance> Request(GameInstance game, Player player, string message)
        => new Implementation(game, player, message);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private GameInstance game { get; }
        private Player player { get; }
        private string message { get; }

        public Implementation(GameInstance game, Player player, string message)
        {
            this.game = game;
            this.player = player;
            this.message = message;
        }

        public override bool CheckPreconditions(Player actor)
            => player == actor && !string.IsNullOrWhiteSpace(message);

        public override void Execute()
        {
            game.ChatLog.Add(new ChatMessage(player, message));
        }

        protected override UnifiedRequestCommandSerializer GetSerializer()
            => new Serializer(player, message);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Player> player;
        private string message;

        public Serializer(Player player, string message)
        {
            this.player = player.Id;
            this.message = message;
        }

        [UsedImplicitly]
        public Serializer() {}

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            => new Implementation(game, game.PlayerFor(player), message);

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref player);
            stream.Serialize(ref message);
        }
    }
}
