using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Events
{
    struct TileInfoChanged
    {
        public Tile Tile { get; }

        public TileInfoChanged(Tile tile)
        {
            Tile = tile;
        }
    }
}
