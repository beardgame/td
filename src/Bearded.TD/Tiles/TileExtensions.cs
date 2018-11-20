using System.Collections.Generic;
using System.Linq;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Tiles
{
    static class TileExtensions
    {
        public static bool OverlapsWithTiles(this IEnumerable<Tile> tiles, IEnumerable<Tile> otherTiles)
            => tiles.Any(otherTiles.Contains);

        public static bool NeighboursToTiles(this Tile tile, IEnumerable<Tile> otherTiles)
            => tile.Neighbours.Prepend(tile).OverlapsWithTiles(otherTiles);
    }
}
