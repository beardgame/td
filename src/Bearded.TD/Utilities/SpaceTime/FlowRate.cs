using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities.SpaceTime;

struct FlowRate : IMeasure1
{
    private readonly double value;

    public double NumericValue => value;

    public FlowRate(double value)
    {
        this.value = value;
    }

    public static Volume operator *(FlowRate flowRate, TimeSpan timeSpan)
    {
        return new Volume(flowRate.value * timeSpan.NumericValue);
    }
}