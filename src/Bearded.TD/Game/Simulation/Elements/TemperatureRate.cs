using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

readonly struct TemperatureRate
{
    public static TemperatureRate Zero { get; } = new(0);

    public float NumericValue { get; }

    public TemperatureRate(float value)
    {
        NumericValue = value;
    }

    public bool Equals(TemperatureRate other) => NumericValue.Equals(other.NumericValue);

    public override bool Equals(object? obj) => obj is TemperatureRate other && Equals(other);

    public override int GetHashCode() => NumericValue.GetHashCode();

    public override string ToString() => $"{NumericValue}°/s";

    public static bool operator ==(TemperatureRate left, TemperatureRate right) => left.Equals(right);

    public static bool operator !=(TemperatureRate left, TemperatureRate right) => !left.Equals(right);

    public static bool operator <(TemperatureRate left, TemperatureRate right) =>
        left.NumericValue < right.NumericValue;

    public static bool operator <=(TemperatureRate left, TemperatureRate right) =>
        left.NumericValue <= right.NumericValue;

    public static bool operator >(TemperatureRate left, TemperatureRate right) =>
        left.NumericValue > right.NumericValue;

    public static bool operator >=(TemperatureRate left, TemperatureRate right) =>
        left.NumericValue >= right.NumericValue;

    public static TemperatureRate operator +(TemperatureRate left, TemperatureRate right) =>
        new(left.NumericValue + right.NumericValue);

    public static TemperatureRate operator -(TemperatureRate left, TemperatureRate right) =>
        new(left.NumericValue - right.NumericValue);

    public static TemperatureRate operator *(float scalar, TemperatureRate rate) => new(scalar * rate.NumericValue);

    public static TemperatureRate operator *(TemperatureRate rate, float scalar) => scalar * rate;

    public static TemperatureDifference operator *(TimeSpan duration, TemperatureRate rate) =>
        new((float) (duration.NumericValue * rate.NumericValue));

    public static TemperatureDifference operator *(TemperatureRate rate, TimeSpan duration) => duration * rate;
}
