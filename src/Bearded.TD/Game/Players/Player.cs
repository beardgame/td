using amulware.Graphics;

namespace Bearded.TD.Game.Players
{
    sealed class Player
    {
        public Color Color { get; }

        public Player(Color color)
        {
            Color = color;
        }
    }
}