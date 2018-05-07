namespace Bearded.UI
{
    public struct Interval
    {
        public double Start { get; }
        public double End { get; }
        public double Size => End - Start;

        private Interval(double start, double end)
        {
            Start = start;
            End = end;
        }

        public static Interval FromStartAndEnd(double start, double end) => new Interval(start, end);
        public static Interval FromStartAndSize(double start, double size) => new Interval(start, start + size);
    }
}