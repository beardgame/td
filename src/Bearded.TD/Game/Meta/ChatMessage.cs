using Bearded.TD.Game.Players;

namespace Bearded.TD.Game.Meta
{
    sealed class ChatMessage
    {
        public Player? Player { get; }
        public string Text { get; }

        public ChatMessage(Player? player, string text)
        {
            Player = player;
            Text = text;
        }

        public string GetDisplayString() => Player == null
            ? $"[System] {Text}"
            : $"{Player.Name}: {Text}";
    }
}
