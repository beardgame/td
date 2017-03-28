using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.UI;

namespace Bearded.TD.Game.Commands
{
    static class SendChatMessage
    {
        public static IRequest Request(GameInstance game, Player player, string message)
            => new Implementation(game, player, message);

        private class Implementation : UnifiedDebugRequestCommand
        {
            private readonly GameInstance game;
            private readonly Player player;
            private readonly string message;

            public Implementation(GameInstance game, Player player, string message)
            {
                this.game = game;
                this.player = player;
                this.message = message;
            }

            protected override bool CheckPreconditionsDebug()
                => player != null && !string.IsNullOrWhiteSpace(message);

            public override void Execute()
            {
                game.ChatLog.Add(new ChatMessage(player, message));
            }
        }
    }
}