using System;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities.SpaceTime
{
    readonly struct Circle : IEquatable<Circle>
    {
        public Position2 Center { get; }
        public Unit Radius { get; }

        public Circle(Position2 center, Unit radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool Equals(Circle other) => Center.Equals(other.Center) && Radius.Equals(other.Radius);
        public override bool Equals(object? obj) => obj is Circle other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Center, Radius);

        public static bool operator ==(Circle left, Circle right) => left.Equals(right);
        public static bool operator !=(Circle left, Circle right) => !left.Equals(right);
    }
}
