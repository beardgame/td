using System;
using Bearded.TD.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Resources
{
    readonly struct ResourceAmount : IMeasure1, IEquatable<ResourceAmount>
    {
        public static ResourceAmount Zero { get; } = new(0);

        public double NumericValue { get; }

        public long DisplayValue => (long) NumericValue;

        public ResourceAmount(double numericValue)
        {
            NumericValue = numericValue;
        }

        public bool Equals(ResourceAmount other) => NumericValue.Equals(other.NumericValue);

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

        public static ResourceAmount operator *(double scalar, ResourceAmount amount) =>
            new(scalar * amount.NumericValue);

        public static ResourceAmount operator /(ResourceAmount amount, double scalar) =>
            new(amount.NumericValue / scalar);

        public static double operator /(ResourceAmount left, ResourceAmount right) =>
            left.NumericValue / right.NumericValue;

        public static ResourceRate operator /(ResourceAmount amount, TimeSpan time) =>
            new(amount.NumericValue / time.NumericValue);
    }

    static class ResourceAmountExtensions
    {
        public static ResourceAmount Resources(this double amount) => new(amount);
        public static ResourceAmount Resources(this int amount) => new(amount);
    }
}
