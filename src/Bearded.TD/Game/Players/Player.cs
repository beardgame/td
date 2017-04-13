using amulware.Graphics;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Players
{
    sealed class Player
    {
        public Id<Player> Id { get; }

        public Color Color { get; }

        public Player(Id<Player> id, Color color)
        {
            Id = id;
            Color = color;
        }
    }
}