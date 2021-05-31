using System;

namespace Bearded.TD.Tiles
{
    readonly struct Tile
    {
        public static Tile Origin = new(0, 0);

        public int X { get; }
        public int Y { get; }

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int Radius => X * Y >= 0
            ? Math.Abs(X + Y)
            : Math.Max(Math.Abs(X), Math.Abs(Y));

        public TileEdge Edge(Direction direction) => TileEdge.From(this, direction);
        public Tile Neighbour(Direction direction) => Offset(direction.Step());
        public Tile Offset(Step step) => new(X + step.X, Y + step.Y);

        public int DistanceTo(Tile other)
        {
            if (Equals(other)) return 0;

            var diffHorizontal = other.X - X; // -
            var diffVertical = other.Y - Y; // \

            var diffHorizontalAbs = Math.Abs(diffHorizontal);
            var diffVerticalAbs = Math.Abs(diffVertical);

            // Combination of \ and - can be combined into /
            var reduction = 0;
            // faster than two calls to Math.Sign
            if (diffHorizontal * (long)diffVertical < 0)
                reduction = Math.Min(diffHorizontalAbs, diffVerticalAbs);
            return diffHorizontalAbs + diffVerticalAbs - reduction;
        }

        public override bool Equals(object obj)
            => obj is Tile tile && Equals(tile);

        public bool Equals(Tile other)
            => X == other.X && Y == other.Y;

        public override int GetHashCode() => (X * 397) ^ Y;

        public static bool operator ==(Tile t1, Tile t2) => t1.Equals(t2);
        public static bool operator !=(Tile t1, Tile t2) => !(t1 == t2);

        public override string ToString()
            => $"{X},{Y}";

        /*
         *    -z
         * +y    +x
         * -z    -y
         *    +z
         */
        public (int X, int Y, int Z) ToXYZ()
        {
            var yy = -X;
            var zz = -Y;
            var xx = -yy - zz;

            return (xx, yy, zz);
        }

        public static Tile FromXYZ(int _, int y, int z) => new(-y, -z);

        public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);
    }
}
