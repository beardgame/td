namespace Bearded.TD.Utilities.SpaceTime
{
    static class SpaceTime1Math
    {
        public static T Min<T>(T left, T right) where T : IMeasure1 =>
            left.NumericValue <= right.NumericValue ? left : right;

        public static T Max<T>(T left, T right) where T : IMeasure1 =>
            left.NumericValue >= right.NumericValue ? left : right;
    }
}
