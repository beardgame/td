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

        public override bool Equals(object obj) => obj is Vector2i v && Equals(v);

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public static bool operator ==(Vector2i left, Vector2i right) => left.Equals(right);

        public static bool operator !=(Vector2i left, Vector2i right) => !left.Equals(right);

        public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);

        public static implicit operator Vector2i((int X, int Y) tuple) => new Vector2i(tuple.X, tuple.Y);
        public static implicit operator (int X, int Y)(Vector2i v) => (v.X, v.Y);
    }
}
