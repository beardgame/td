using System.Collections.Generic;
using System.Linq;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Tiles
{
    static class TileExtensions
    {
        public static bool OverlapsWithTiles<TTileInfo>(
            this IEnumerable<Tile<TTileInfo>> tiles, IEnumerable<Tile<TTileInfo>> otherTiles)
            => tiles.Any(otherTiles.Contains);

        public static bool NeighboursToTiles<TTileInfo>(
            this Tile<TTileInfo> tile, IEnumerable<Tile<TTileInfo>> otherTiles)
            => tile.Neighbours.Prepend(tile).OverlapsWithTiles(otherTiles);
    }
}
