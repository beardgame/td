using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.UI;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SendChatMessage
    {
        public static IRequest Request(GameInstance game, Player player, string message)
            => new Implementation(game, player, message);

        private class Implementation : UnifiedRequestCommand
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

            public override bool CheckPreconditions()
                => player != null && !string.IsNullOrWhiteSpace(message);

            public override void Execute()
            {
                game.ChatLog.Add(new ChatMessage(player, message));
            }

            protected override UnifiedRequestCommandSerializer GetSerializer()
                => new Serializer(player, message);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Player> player;
            private string message;

            public Serializer(Player player, string message)
            {
                this.player = player.Id;
                this.message = message;
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game, Player sender)
                => new Implementation(game, game.PlayerFor(player), message);

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref player);
                stream.Serialize(ref message);
            }
        }
    }
}