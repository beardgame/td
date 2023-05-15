using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

readonly struct Temperature : IMeasure1F
{
    public static Temperature Zero { get; } = new(0);

    public float NumericValue { get; }

    public Temperature(float value)
    {
        NumericValue = value;
    }

    public bool Equals(Temperature other) => NumericValue.Equals(other.NumericValue);

    public override bool Equals(object? obj) => obj is Temperature other && Equals(other);

    public override int GetHashCode() => NumericValue.GetHashCode();

    public override string ToString() => $"{NumericValue}°";

    public static bool operator ==(Temperature left, Temperature right) => left.Equals(right);

    public static bool operator !=(Temperature left, Temperature right) => !left.Equals(right);

    public static bool operator <(Temperature left, Temperature right) => left.NumericValue < right.NumericValue;

    public static bool operator <=(Temperature left, Temperature right) =>
        left.NumericValue <= right.NumericValue;

    public static bool operator >(Temperature left, Temperature right) => left.NumericValue > right.NumericValue;

    public static bool operator >=(Temperature left, Temperature right) =>
        left.NumericValue >= right.NumericValue;

    public static Temperature operator +(Temperature t, TemperatureDifference diff) =>
        new(t.NumericValue + diff.NumericValue);

    public static Temperature operator +(TemperatureDifference diff, Temperature t) => t + diff;

    public static Temperature operator -(Temperature t, TemperatureDifference diff) =>
        new(t.NumericValue - diff.NumericValue);

    public static TemperatureDifference operator -(Temperature t1, Temperature t2) =>
        new(t1.NumericValue - t2.NumericValue);
}
