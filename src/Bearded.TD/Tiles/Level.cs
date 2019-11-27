using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;
using static System.Math;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Tiles
{
    sealed class Level
    {
        public int Radius { get; }

        public Tile Center { get; } = Tile.Origin;
        public IList<Tile> Corners { get; }

        public Level(int radius)
        {
            Radius = radius;
            Corners = Directions
                .All
                .Enumerate()
                .Select(dir => Tile.Origin.Offset(dir.Step() * Radius))
                .ToList();
        }

        public bool IsValid(Tile tile)
        {
            return tile.X >= -Radius && tile.X <= Radius
                && tile.Y >= -Radius && tile.Y <= Radius
                && (tile.X + tile.Y) >= -Radius && (tile.X + tile.Y) <= Radius;
        }

        public IEnumerable<Direction> ValidDirectionsFrom(Tile tile)
        {
            return tile.Radius < Radius
                ? Tilemap.Directions
                : Tilemap.Directions.Where(d => tile.Neighbour(d).Radius <= Radius);
        }

        public IEnumerable<Tile> ValidNeighboursOf(Tile tile)
        {
            return ValidDirectionsFrom(tile).Select(tile.Neighbour);
        }

        public static Tile GetTile(Position3 position)
        {
            return GetTile(position.XY());
        }

        public static Tile GetTile(Position2 position)
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
            var isBottomLeftCorner = -xRemainder > yRemainder;

            tx += isBottomRightCorner ? 1 : 0;
            ty += isBottomRightCorner || isBottomLeftCorner ? -1 : 0;

            return new Tile(tx, ty);
        }

        public static Position2 GetPosition(Tile tile)
            => new Position2(
                (tile.X + tile.Y * 0.5f) * HexagonDistanceX,
                tile.Y * HexagonDistanceY
            );

        public static IEnumerable<Tile> TilesInCircle(Position2 center, Unit radius)
        {
            return Tilemap.GetSpiralCenteredAt(GetTile(center), (int) Ceiling(radius.NumericValue / HexagonWidth));
        }
    }
}
