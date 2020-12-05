using System;
using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Resources
{
    readonly struct ResourceAmount : IDiscreteMeasure1, IEquatable<ResourceAmount>
    {
        public static ResourceAmount Zero { get; } = new(0);

        public long NumericValue { get; }

        public ResourceAmount(long numericValue)
        {
            NumericValue = numericValue;
        }

        // NB: this multiplication always rounds down.
        public ResourceAmount DiscretizedPercentage(double factor) => new((long) (factor * NumericValue));

        public bool Equals(ResourceAmount other) => NumericValue == other.NumericValue;

        public override bool Equals(object? obj) => obj is ResourceAmount other && Equals(other);

        public override int GetHashCode() => NumericValue.GetHashCode();

        public static bool operator ==(ResourceAmount left, ResourceAmount right) => left.Equals(right);

        public static bool operator !=(ResourceAmount left, ResourceAmount right) => !left.Equals(right);

        public static bool operator <(ResourceAmount left, ResourceAmount right) =>
            left.NumericValue < right.NumericValue;

        public static bool operator <=(ResourceAmount left, ResourceAmount right) =>
            left.NumericValue <= right.NumericValue;

        public static bool operator >(ResourceAmount left, ResourceAmount right) =>
            left.NumericValue > right.NumericValue;

        public static bool operator >=(ResourceAmount left, ResourceAmount right) =>
            left.NumericValue >= right.NumericValue;

        public static ResourceAmount operator +(ResourceAmount left, ResourceAmount right) =>
            new(left.NumericValue + right.NumericValue);

        public static ResourceAmount operator -(ResourceAmount left, ResourceAmount right) =>
            new(left.NumericValue - right.NumericValue);

        public static ResourceAmount operator *(long scalar, ResourceAmount amount) =>
            new(scalar * amount.NumericValue);

        public static double operator /(ResourceAmount left, ResourceAmount right) =>
            (double) left.NumericValue / right.NumericValue;
    }

    static class ResourceAmountExtensions
    {
        public static ResourceAmount Resources(this long amount) => new(amount);

        public static ResourceAmount Resources(this int amount) => new(amount);
    }
}
