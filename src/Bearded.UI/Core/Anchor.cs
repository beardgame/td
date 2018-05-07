namespace Bearded.UI
{
    public struct Anchor
    {
        public double Percentage { get; }
        public double Offset { get; }

        public Anchor(double percentage, double offset)
        {
            Percentage = percentage;
            Offset = offset;
        }

        public double CalculatePointWithin(Interval interval)
            => interval.Start + Percentage * interval.Size + Offset;
    }
}
