using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Tiles
{
    static class TileExtensions
    {
        public static TileEdge Edge (this Tile tile, Direction direction) => TileEdge.From(tile, direction);
        public static Tile Offset(this Tile tile, Step offset) => tile + offset;

        public static bool OverlapsWithTiles(this IEnumerable<Tile> tiles, IEnumerable<Tile> otherTiles)
            => tiles.Any(otherTiles.Contains);

        public static bool NeighboursToTiles(this Tile tile, IEnumerable<Tile> otherTiles)
            => tile.PossibleNeighbours().Prepend(tile).OverlapsWithTiles(otherTiles);

        public static Tile RotatedCounterClockwiseAroundOrigin(this Tile tile)
        {
            var (x, y, z) = tile.ToXYZ();
            return ToTile(-z, -x, -y);
        }

        public static Tile RotatedClockwiseAroundOrigin(this Tile tile)
        {
            var (x, y, z) = tile.ToXYZ();
            return ToTile(-y, -z, -x);
        }

        /*
         *    -z
         * +y    +x
         * -z    -y
         *    +z
         */
        public static (int X, int Y, int Z) ToXYZ(this Tile tile)
        {
            var yy = -tile.X;
            var zz = -tile.Y;
            var xx = -yy - zz;

            return (xx, yy, zz);
        }

        // TODO: I'm quite sure this is broken
        // because when I inline it into RotatedCounterClockwiseAroundOrigin
        // that method appears to turn into a no-op
        // This whole class needs testing if actually still need it :)
        public static Tile ToTile(int _, int y, int z) => new(-y, -z);
    }
}
