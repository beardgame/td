using System;

namespace Bearded.TD.Tiles
{
    readonly struct TileCorner : IEquatable<TileCorner>
    {
        private enum Corner
        {
            UpRight = 0, Up = 1
        }

        private readonly Tile tile;
        private readonly Corner corner;

        public static TileCorner FromTileAndDirectionBefore(Tile tile, Direction direction)
        {
            return direction switch
            {
                Direction.Right => new(tile, Corner.UpRight),
                Direction.UpRight => new(tile, Corner.Up),
                Direction.UpLeft => new(tile.Neighbour(Direction.Left), Corner.UpRight),
                Direction.Left => new(tile.Neighbour(Direction.DownLeft), Corner.Up),
                Direction.DownLeft => new(tile.Neighbour(Direction.DownLeft), Corner.UpRight),
                Direction.DownRight => new(tile.Neighbour(Direction.DownRight), Corner.Up),
                Direction.Unknown or _ =>
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        private TileCorner(Tile tile, Corner corner)
        {
            this.tile = tile;
            this.corner = corner;
        }



        public static bool operator ==(TileCorner left, TileCorner right) => left.Equals(right);
        public static bool operator !=(TileCorner left, TileCorner right) => !left.Equals(right);
        public override bool Equals(object? obj) => obj is TileCorner other && Equals(other);
        public bool Equals(TileCorner other) => tile.Equals(other.tile) && corner == other.corner;
        public override int GetHashCode() => HashCode.Combine(tile, (int) corner);
    }
}
