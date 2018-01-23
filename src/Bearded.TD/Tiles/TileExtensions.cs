using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.World;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Tiles
{
    static class TileExtensions
    {
        public static bool OverlapsWithTiles(
            this IEnumerable<Tile<TileInfo>> tiles, IEnumerable<Tile<TileInfo>> otherTiles)
            => tiles.Any(otherTiles.Contains);

        public static bool NeighboursToTiles(
            this Tile<TileInfo> tile, IEnumerable<Tile<TileInfo>> otherTiles)
            => tile.Neighbours.Prepend(tile).OverlapsWithTiles(otherTiles);
    }
}
