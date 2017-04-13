using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.UI;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SendChatMessage
    {
        public static IRequest Request(GameInstance game, Player player, string message)
            => new Implementation(game, player, message);

        public static ICommandSerializer GetCommandSerializer() => null;

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

            protected override IUnifiedRequestCommandSerializer GetSerializer()
                => new Serializer(player, message);
        }

        private class Serializer : IUnifiedRequestCommandSerializer
        {
            private Id Player;
            private string Message;

            public Serializer(Player player, string message)
            {
                Player = player.Id.Simple;
                Message = message;
            }

            public Serializer()
            {
            }

            public IRequest GetRequest(GameInstance game)
                => SendChatMessage.Request(game, null, Message);

            public ICommand GetCommand(GameInstance game)
                => GetRequest(game).ToCommand();

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref Player);
                stream.Serialize(ref Message);
            }
        }
    }
}