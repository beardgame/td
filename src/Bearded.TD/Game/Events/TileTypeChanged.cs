using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Events
{
    struct TileTypeChanged : IEvent
    {
        public Tile Tile { get; }
        public TileGeometry.TileType Type { get; }

        public TileTypeChanged(Tile tile, TileGeometry.TileType type)
        {
            Tile = tile;
            Type = type;
        }
    }
}