using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities.SpaceTime;

struct EnergyConsumptionRate
{
    public double NumericValue { get; }

    public EnergyConsumptionRate(double numericValue)
    {
        NumericValue = numericValue;
    }

    public static Energy operator *(EnergyConsumptionRate energy, TimeSpan timeSpan) =>
        new Energy(energy.NumericValue * timeSpan.NumericValue);

    public static Energy operator *(TimeSpan timeSpan, EnergyConsumptionRate energy) => energy * timeSpan;
}