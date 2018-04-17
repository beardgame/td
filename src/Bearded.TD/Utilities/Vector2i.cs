using System;

namespace Bearded.TD.Utilities
{
    struct Vector2i : IEquatable<Vector2i>
    {
        public int X { get; }
        public int Y { get; }

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Vector2i other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj)
            => obj is Vector2i asVector2I && Equals(asVector2I);

        public override int GetHashCode() => (X * 397) ^ Y;

        public static bool operator ==(Vector2i left, Vector2i right) => left.Equals(right);
        public static bool operator !=(Vector2i left, Vector2i right) => !left.Equals(right);

        public override string ToString() => $"({X},{Y})";
    }
}