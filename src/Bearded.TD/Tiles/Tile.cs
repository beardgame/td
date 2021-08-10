using System;

namespace Bearded.TD.Tiles
{
    public readonly struct Tile : IEquatable<Tile>
    {
        public static Tile Origin => new(0, 0);

        public int X { get; }
        public int Y { get; }

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int Radius => Step.FromOriginTowards(this).Distance;
        public int DistanceTo(Tile other) => Step.Between(this, other).Distance;

        public Tile Neighbor(Direction direction) => this + direction.Step();

        public bool Equals(Tile other) => X == other.X && Y == other.Y;

        public override bool Equals(object? obj) => obj is Tile other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(Tile left, Tile right) => left.Equals(right);

        public static bool operator !=(Tile left, Tile right) => !left.Equals(right);

        public static Step operator -(Tile target, Tile origin) => Step.Between(origin, target);

        public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);

        public override string ToString() => $"{X},{Y}";
    }
}
