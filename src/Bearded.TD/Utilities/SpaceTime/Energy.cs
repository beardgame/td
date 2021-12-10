namespace Bearded.TD.Utilities.SpaceTime;

struct Energy : IMeasure1
{
    public static Energy Zero => new Energy(0);

    public static Energy Infinite => new Energy(double.PositiveInfinity);

    public double NumericValue { get; }

    public Energy(double numericValue)
    {
        NumericValue = numericValue;
    }

    public static Energy operator +(Energy left, Energy right) =>
        new Energy(left.NumericValue + right.NumericValue);

    public static Energy operator -(Energy left, Energy right) =>
        new Energy(left.NumericValue - right.NumericValue);

    public static Energy operator *(Energy energy, double d) => new Energy(energy.NumericValue * d);

    public static Energy operator *(double d, Energy energy) => energy * d;

    public static bool operator <(Energy left, Energy right) => left.NumericValue < right.NumericValue;

    public static bool operator <=(Energy left, Energy right) => left.NumericValue <= right.NumericValue;

    public static bool operator >(Energy left, Energy right) => left.NumericValue > right.NumericValue;

    public static bool operator >=(Energy left, Energy right) => left.NumericValue >= right.NumericValue;
}