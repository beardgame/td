using System;

namespace Bearded.TD.Utilities
{
    struct XY : IEquatable<XY>
    {
        public int X { get; }
        public int Y { get; }

        public XY(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(XY other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj) && (obj is XY && Equals((XY) obj));

        public override int GetHashCode() => (X * 397) ^ Y;

        public static bool operator ==(XY left, XY right) => left.Equals(right);
        public static bool operator !=(XY left, XY right) => !left.Equals(right);

        public override string ToString() => $"({X},{Y})";
    }
}