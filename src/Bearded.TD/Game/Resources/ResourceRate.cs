using System;
using Bearded.TD.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Resources
{
    readonly struct ResourceRate : IDiscreteMeasure1, IEquatable<ResourceRate>
    {
        public static ResourceRate Zero { get; } = new(0);

        public long NumericValue { get; }

        public ResourceRate(long numericValue)
        {
            NumericValue = numericValue;
        }

        // NB: this multiplication always rounds down. It shouldn't be used to calculate the diff in resources each
        // frame. That is why it is implemented as a static method and not an operator.
        public ResourceAmount DiscretizedAmountInTime(TimeSpan timeSpan) =>
            new((long) (timeSpan.NumericValue * NumericValue));

        public bool Equals(ResourceRate other) => NumericValue == other.NumericValue;

        public override bool Equals(object? obj) => obj is ResourceRate other && Equals(other);

        public override int GetHashCode() => NumericValue.GetHashCode();

        public static bool operator ==(ResourceRate left, ResourceRate right) => left.Equals(right);

        public static bool operator !=(ResourceRate left, ResourceRate right) => !left.Equals(right);

        public static double operator /(ResourceRate left, ResourceRate right) =>
            (double) left.NumericValue / right.NumericValue;

        public static bool operator <(ResourceRate left, ResourceRate right) => left.NumericValue < right.NumericValue;

        public static bool operator <=(ResourceRate left, ResourceRate right) =>
            left.NumericValue <= right.NumericValue;

        public static bool operator >(ResourceRate left, ResourceRate right) => left.NumericValue > right.NumericValue;

        public static bool operator >=(ResourceRate left, ResourceRate right) =>
            left.NumericValue >= right.NumericValue;

        public static ResourceRate operator +(ResourceRate left, ResourceRate right) =>
            new(left.NumericValue + right.NumericValue);

        public static ResourceRate operator -(ResourceRate left, ResourceRate right) =>
            new(left.NumericValue - right.NumericValue);

        public static ResourceRate operator *(long scalar, ResourceRate amount) =>
            new(scalar * amount.NumericValue);
    }

    static class ResourceRateExtensions
    {
        public static ResourceRate ResourcesPerSecond(this long amount) => new(amount);

        public static ResourceRate ResourcesPerSecond(this int amount) => new(amount);
    }
}
