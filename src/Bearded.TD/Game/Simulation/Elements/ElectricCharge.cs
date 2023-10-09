using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

readonly struct ElectricCharge : IMeasure1F
{
    public static ElectricCharge Zero { get; } = new(0);

    public float NumericValue { get; }

    public ElectricCharge(float value)
    {
        NumericValue = value;
    }

    public bool Equals(ElectricCharge other) => NumericValue.Equals(other.NumericValue);

    public override bool Equals(object? obj) => obj is ElectricCharge other && Equals(other);

    public override int GetHashCode() => NumericValue.GetHashCode();

    public override string ToString() => $"{NumericValue:F1}";

    public static bool operator ==(ElectricCharge left, ElectricCharge right) => left.Equals(right);

    public static bool operator !=(ElectricCharge left, ElectricCharge right) => !left.Equals(right);

    public static bool operator <(ElectricCharge left, ElectricCharge right) => left.NumericValue < right.NumericValue;

    public static bool operator <=(ElectricCharge left, ElectricCharge right) =>
        left.NumericValue <= right.NumericValue;

    public static bool operator >(ElectricCharge left, ElectricCharge right) => left.NumericValue > right.NumericValue;

    public static bool operator >=(ElectricCharge left, ElectricCharge right) =>
        left.NumericValue >= right.NumericValue;

    public static ElectricCharge operator +(ElectricCharge left, ElectricCharge right) =>
        new(left.NumericValue + right.NumericValue);

    public static ElectricCharge operator -(ElectricCharge left, ElectricCharge right) =>
        new(left.NumericValue - right.NumericValue);

    public static ElectricCharge operator *(float scalar, ElectricCharge charge) => new(scalar * charge.NumericValue);

    public static ElectricCharge operator *(ElectricCharge charge, float scalar) => scalar * charge;

    public static float operator /(ElectricCharge left, ElectricCharge right) => left.NumericValue / right.NumericValue;

    public static ElectricChargeRate operator /(ElectricCharge charge, TimeSpan duration) =>
        new(charge.NumericValue / (float) duration.NumericValue);
}
