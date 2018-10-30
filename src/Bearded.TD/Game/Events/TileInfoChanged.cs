
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Events
{
    struct TileInfoChanged
    {
        public Tile<TileInfo> Tile { get; }

        public TileInfoChanged(Tile<TileInfo> tile)
        {
            Tile = tile;
        }
    }
}
