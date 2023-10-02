using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

readonly struct ElectricChargeRate : IMeasure1F
{
    public static ElectricChargeRate Zero { get; } = new(0);

    public float NumericValue { get; }

    public ElectricChargeRate(float value)
    {
        NumericValue = value;
    }

    public bool Equals(ElectricChargeRate other) => NumericValue.Equals(other.NumericValue);

    public override bool Equals(object? obj) => obj is ElectricChargeRate other && Equals(other);

    public override int GetHashCode() => NumericValue.GetHashCode();

    public override string ToString() => $"{NumericValue:F1}/s";

    public static bool operator ==(ElectricChargeRate left, ElectricChargeRate right) => left.Equals(right);

    public static bool operator !=(ElectricChargeRate left, ElectricChargeRate right) => !left.Equals(right);

    public static bool operator <(ElectricChargeRate left, ElectricChargeRate right) =>
        left.NumericValue < right.NumericValue;

    public static bool operator <=(ElectricChargeRate left, ElectricChargeRate right) =>
        left.NumericValue <= right.NumericValue;

    public static bool operator >(ElectricChargeRate left, ElectricChargeRate right) =>
        left.NumericValue > right.NumericValue;

    public static bool operator >=(ElectricChargeRate left, ElectricChargeRate right) =>
        left.NumericValue >= right.NumericValue;

    public static ElectricChargeRate operator +(ElectricChargeRate left, ElectricChargeRate right) =>
        new(left.NumericValue + right.NumericValue);

    public static ElectricChargeRate operator -(ElectricChargeRate left, ElectricChargeRate right) =>
        new(left.NumericValue - right.NumericValue);

    public static ElectricCharge operator *(TimeSpan duration, ElectricChargeRate rate)
        => new((float) duration.NumericValue * rate.NumericValue);

    public static ElectricCharge operator *(ElectricChargeRate rate, TimeSpan duration) => duration * rate;
}
