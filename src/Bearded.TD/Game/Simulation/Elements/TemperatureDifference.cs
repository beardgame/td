namespace Bearded.TD.Game.Simulation.Elements;

readonly struct TemperatureDifference
{
    public static TemperatureDifference Zero { get; } = new(0);

    public float NumericValue { get; }

    public TemperatureDifference(float value)
    {
        NumericValue = value;
    }
}
