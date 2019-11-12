using System;

namespace Bearded.TD.Tiles
{
    struct Tile
    {
        public static Tile Origin = new Tile(0, 0);

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

        public Tile Neighbour(Direction direction) => Offset(direction.Step());
        public Tile Offset(Step step) => new Tile(X + step.X, Y + step.Y);

        public int DistanceTo(Tile other)
        {
            if (Equals(other)) return 0;

            var diffHorizontal = other.X - X; // -
            var diffVertical = other.Y - Y; // \

            // Combination of \ and - can be combined into /
            var reduction = 0;
            if (Math.Sign(diffHorizontal) != Math.Sign(diffVertical))
                reduction = Math.Min(Math.Abs(diffHorizontal), Math.Abs(diffVertical));
            return Math.Abs(diffHorizontal) + Math.Abs(diffVertical) - reduction;
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

        public static Tile FromXYZ(int _, int y, int z) => new Tile(-y, -z);

        public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);
    }
}
