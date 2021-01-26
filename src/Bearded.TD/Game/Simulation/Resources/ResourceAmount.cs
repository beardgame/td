using System;
using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Resources
{
    readonly struct ResourceAmount : IDiscreteMeasure1, IEquatable<ResourceAmount>
    {
        public static ResourceAmount Zero { get; } = new(0);

        public int NumericValue { get; }

        public ResourceAmount(int numericValue)
        {
            NumericValue = numericValue;
        }

        public ResourceAmount Percentage(double percentage) => new((int) (percentage * NumericValue));

        public bool Equals(ResourceAmount other) => NumericValue.Equals(other.NumericValue);

        public override bool Equals(object? obj) => obj is ResourceAmount other && Equals(other);

        public override int GetHashCode() => NumericValue.GetHashCode();

        public override string ToString() => $"{NumericValue} resources";

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

        public static ResourceAmount operator *(int scalar, ResourceAmount amount) =>
            new(scalar * amount.NumericValue);

        public static ResourceAmount operator *(ResourceAmount amount, int scalar) =>
            new(scalar * amount.NumericValue);

        public static double operator /(ResourceAmount left, ResourceAmount right) =>
            (double) left.NumericValue / right.NumericValue;
    }

    static class ResourceAmountExtensions
    {
        public static ResourceAmount Resources(this int amount) => new(amount);
    }
}
