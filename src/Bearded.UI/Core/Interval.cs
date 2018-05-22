namespace Bearded.UI
{
    public struct Interval
    {
        public double Start { get; }
        public double Size { get; }
        public double End => Start + Size;

        private Interval(double start, double size)
        {
            Start = start;
            Size = size;
        }

        public static Interval FromStartAndEnd(double start, double end) => new Interval(start, end - start);
        public static Interval FromStartAndSize(double start, double size) => new Interval(start, size);
    }
}
