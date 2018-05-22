using Bearded.Utilities.SpaceTime;
using static System.Math;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Tiles
{
    class Level<TTileInfo>
    {
        public Tilemap<TTileInfo> Tilemap { get; }

        public Level(Tilemap<TTileInfo> tilemap)
        {
            Tilemap = tilemap;
        }

        public Tile<TTileInfo> GetTile(Position2 position)
        {
            var yf = position.Y.NumericValue * (1 / HexagonDistanceY) + 1 / 1.5f;
            var y = Floor(yf);
            var xf = position.X.NumericValue * (1 / HexagonWidth) - y * 0.5f + 0.5f;
            var x = Floor(xf);

            var xRemainder = xf - x - 0.5f;
            var yRemainder = (yf - y) * 1.5f;

            var tx = (int)x;
            var ty = (int)y;

            var isBottomRightCorner = xRemainder > yRemainder;
            var isBottomLeftConer = -xRemainder > yRemainder;

            tx += isBottomRightCorner ? 1 : 0;
            ty += isBottomRightCorner || isBottomLeftConer ? -1 : 0;

            return new Tile<TTileInfo>(Tilemap, tx, ty);
        }

        public Position2 GetPosition(Tile<TTileInfo> tile)
            => new Position2(
                (tile.X + tile.Y * 0.5f) * HexagonDistanceX,
                tile.Y * HexagonDistanceY
            );
    }
}
