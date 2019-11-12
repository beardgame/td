namespace Bearded.TD.Utilities.SpaceTime
{
    struct Volume
    {
        public static Volume Zero => new Volume(0);

        private readonly float value;

        public float NumericValue => value;

        public Volume(float value)
        {
            this.value = value;
        }

        public static bool operator <(Volume left, Volume right) => left.NumericValue < right.NumericValue;

        public static bool operator <=(Volume left, Volume right) => left.NumericValue <= right.NumericValue;

        public static bool operator >(Volume left, Volume right) => left.NumericValue > right.NumericValue;

        public static bool operator >=(Volume left, Volume right) => left.NumericValue >= right.NumericValue;
    }
}
