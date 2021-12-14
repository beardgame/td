namespace Bearded.TD.Utilities.SpaceTime;

struct Volume : IMeasure1
{
    public static Volume Zero => new Volume(0);

    private readonly double value;

    public double NumericValue => value;

    public Volume(double value)
    {
        this.value = value;
    }

    public static bool operator <(Volume left, Volume right) => left.NumericValue < right.NumericValue;

    public static bool operator <=(Volume left, Volume right) => left.NumericValue <= right.NumericValue;

    public static bool operator >(Volume left, Volume right) => left.NumericValue > right.NumericValue;

    public static bool operator >=(Volume left, Volume right) => left.NumericValue >= right.NumericValue;
}