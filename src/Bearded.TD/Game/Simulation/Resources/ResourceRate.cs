using System;
using Bearded.TD.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Resources;

readonly struct ResourceRate : IDiscreteMeasure1, IEquatable<ResourceRate>
{
    public static ResourceRate Zero { get; } = new(0);

    public int NumericValue { get; }

    public ResourceRate(int numericValue)
    {
        NumericValue = numericValue;
    }

    public ResourceAmount InTime(TimeSpan timeSpan) => new((int) (timeSpan.NumericValue * NumericValue));

    public bool Equals(ResourceRate other) => NumericValue.Equals(other.NumericValue);

    public override bool Equals(object? obj) => obj is ResourceRate other && Equals(other);

    public override int GetHashCode() => NumericValue.GetHashCode();

    public override string ToString() => $"{NumericValue} resources/s";

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

    public static ResourceRate operator *(int scalar, ResourceRate amount) =>
        new(scalar * amount.NumericValue);

    public static ResourceRate operator /(ResourceRate amount, int scalar) =>
        new(amount.NumericValue / scalar);
}

static class ResourceRateExtensions
{
    public static ResourceRate ResourcesPerSecond(this int amount) => new(amount);
}