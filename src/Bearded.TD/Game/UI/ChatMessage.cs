using Bearded.TD.Game.Players;

namespace Bearded.TD.Game.UI
{
    sealed class ChatMessage
    {
        public Player Player { get; }
        public string Text { get; }

        public ChatMessage(Player player, string text)
        {
            Player = player;
            Text = text;
        }
    }
}