using Bearded.TD.Game.GameState.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.GameState.World
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
