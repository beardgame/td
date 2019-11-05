namespace Bearded.TD.Game.Elements
{
    struct Energy
    {
        public static Energy Zero => new Energy(0);

        public double NumericValue { get; }

        public Energy(double numericValue)
        {
            NumericValue = numericValue;
        }

        public static Energy operator +(Energy left, Energy right) =>
            new Energy(left.NumericValue + right.NumericValue);

        public static Energy operator -(Energy left, Energy right) =>
            new Energy(left.NumericValue - right.NumericValue);

        public static bool operator <(Energy left, Energy right) => left.NumericValue < right.NumericValue;

        public static bool operator <=(Energy left, Energy right) => left.NumericValue <= right.NumericValue;

        public static bool operator >(Energy left, Energy right) => left.NumericValue > right.NumericValue;

        public static bool operator >=(Energy left, Energy right) => left.NumericValue >= right.NumericValue;
    }
}
