using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using static System.Math;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.World
{
    sealed class Level
    {
        public Tilemap<TileInfo> Tilemap { get; }

        public Level(Tilemap<TileInfo> tilemap)
        {
            Tilemap = tilemap;
        }

        public Tile<TileInfo> GetTile(Position2 position)
        {
            var yf = position.Y.NumericValue * (1 / HexagonDistanceY) + 1 / 1.5f;
            var y = Floor(yf);
            var xf = position.X.NumericValue * (1 / HexagonWidth) - y * 0.5f + 0.5f;
            var x = Floor(xf);

            var xRemainder = (xf - x) - 0.5f;
            var yRemainder = (yf - y) * 1.5f;

            var tx = (int) x;
            var ty = (int) y;

            if (xRemainder > yRemainder)
            {
                tx++;
                ty--;
            }
            else if (-xRemainder > yRemainder)
                ty--;

            return new Tile<TileInfo>(Tilemap, tx, ty);
        }

        public Position2 GetPosition(Tile<TileInfo> tile)
            => new Position2(
                (tile.X + tile.Y * 0.5f) * HexagonDistanceX,
                tile.Y * HexagonDistanceY
            );

        public void Draw(GeometryManager geos)
        {
            var geo = geos.ConsoleBackground;

            const float w = HexagonDistanceX * 0.5f - 0.1f;
            const float h = HexagonDistanceY * 0.5f - 0.1f;

            foreach (var tile in Tilemap)
            {
                var p = GetPosition(tile).NumericValue;

                geo.Color = Tilemap[tile].IsPassable ? Color.Gray : Color.DarkSlateGray;

                geo.DrawRectangle(p.X - w, p.Y - h, w * 2, h * 2);

            }
        }
    }
}
