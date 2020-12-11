using System;
using Bearded.TD.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Resources
{
    readonly struct ResourceRate : IMeasure1, IEquatable<ResourceRate>
    {
        public static ResourceRate Zero { get; } = new(0);

        public double NumericValue { get; }

        public long DisplayValue => (long) NumericValue;

        public ResourceRate(double numericValue)
        {
            NumericValue = numericValue;
        }

        public bool Equals(ResourceRate other) => NumericValue.Equals(other.NumericValue);

        public override bool Equals(object? obj) => obj is ResourceRate other && Equals(other);

        public override int GetHashCode() => NumericValue.GetHashCode();

        public static bool operator ==(ResourceRate left, ResourceRate right) => left.Equals(right);

        public static bool operator !=(ResourceRate left, ResourceRate right) => !left.Equals(right);

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

        public static ResourceRate operator *(double scalar, ResourceRate amount) =>
            new(scalar * amount.NumericValue);

        public static ResourceRate operator /(ResourceRate amount, double scalar) =>
            new(amount.NumericValue / scalar);

        public static double operator /(ResourceRate left, ResourceRate right) =>
            left.NumericValue / right.NumericValue;

        public static ResourceAmount operator *(ResourceRate rate, TimeSpan time) =>
            new(rate.NumericValue * time.NumericValue);
    }

    static class ResourceRateExtensions
    {
        public static ResourceRate ResourcesPerSecond(this double amount) => new(amount);

        public static ResourceRate ResourcesPerSecond(this int amount) => new(amount);
    }
}
