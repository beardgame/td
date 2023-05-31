using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

readonly struct TemperatureDifference
{
    public static TemperatureDifference Zero { get; } = new(0);

    public float NumericValue { get; }

    public TemperatureDifference(float value)
    {
        NumericValue = value;
    }

    public TemperatureRate PerSecond() => Per(TimeSpan.One);

    public TemperatureRate Per(TimeSpan time) => this / time;

    public bool Equals(Temperature other) => NumericValue.Equals(other.NumericValue);

    public override bool Equals(object? obj) => obj is Temperature other && Equals(other);

    public override int GetHashCode() => NumericValue.GetHashCode();

    public override string ToString() => $"{NumericValue:+#;-#;0}°";

    public static bool operator ==(TemperatureDifference left, TemperatureDifference right) => left.Equals(right);

    public static bool operator !=(TemperatureDifference left, TemperatureDifference right) => !left.Equals(right);

    public static bool operator <(TemperatureDifference left, TemperatureDifference right) =>
        left.NumericValue < right.NumericValue;

    public static bool operator <=(TemperatureDifference left, TemperatureDifference right) =>
        left.NumericValue <= right.NumericValue;

    public static bool operator >(TemperatureDifference left, TemperatureDifference right) =>
        left.NumericValue > right.NumericValue;

    public static bool operator >=(TemperatureDifference left, TemperatureDifference right) =>
        left.NumericValue >= right.NumericValue;

    public static TemperatureDifference operator +(TemperatureDifference left, TemperatureDifference right) =>
        new(left.NumericValue + right.NumericValue);

    public static float operator /(TemperatureDifference d1, TemperatureDifference d2) =>
        d1.NumericValue / d2.NumericValue;

    public static TemperatureRate operator /(TemperatureDifference diff, TimeSpan time) =>
        new((float) (diff.NumericValue / time.NumericValue));
}
