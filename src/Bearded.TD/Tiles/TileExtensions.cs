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
            => tile.PossibleNeighbours().Prepend(tile).OverlapsWithTiles(otherTiles);

        public static Tile RotatedCounterClockwiseAroundOrigin(this Tile tile)
        {
            var (x, y, z) = tile.ToXYZ();
            return Tile.FromXYZ(-z, -x, -y);
        }

        public static Tile RotatedClockwiseAroundOrigin(this Tile tile)
        {
            var (x, y, z) = tile.ToXYZ();
            return Tile.FromXYZ(-y, -z, -x);
        }
    }
}
