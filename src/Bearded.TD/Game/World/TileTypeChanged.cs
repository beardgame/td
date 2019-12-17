using Bearded.TD.Game.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.World
{
    struct TileTypeChanged : IGlobalEvent
    {
        public Tile Tile { get; }
        public TileType Type { get; }

        public TileTypeChanged(Tile tile, TileType type)
        {
            Tile = tile;
            Type = type;
        }
    }
}
