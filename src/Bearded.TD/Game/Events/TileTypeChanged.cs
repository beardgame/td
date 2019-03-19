using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Events
{
    struct TileTypeChanged : IEvent
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