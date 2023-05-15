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

    public static TemperatureRate operator *(float scalar, TemperatureRate rate) => new(scalar * rate.NumericValue);

    public static TemperatureDifference operator *(TimeSpan duration, TemperatureRate rate) =>
        new((float) (duration.NumericValue * rate.NumericValue));

    public static TemperatureDifference operator *(TemperatureRate rate, TimeSpan duration) => duration * rate;
}
