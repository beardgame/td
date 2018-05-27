namespace Bearded.UI
{
    public struct Anchors
    {
        public Anchor Start { get; }
        public Anchor End { get; }

        public Anchors(Anchor start, Anchor end)
        {
            Start = start;
            End = end;
        }

        public Interval CalculateIntervalWithin(Interval parent)
            => Interval.FromStartAndEnd(Start.CalculatePointWithin(parent), End.CalculatePointWithin(parent));

        public HorizontalAnchors H => new HorizontalAnchors(this);
        public VerticalAnchors V => new VerticalAnchors(this);

        public static Anchors Default => new Anchors(new Anchor(0, 0), new Anchor(1, 0));

        public static explicit operator HorizontalAnchors(Anchors anchors) => anchors.H;
        public static explicit operator VerticalAnchors(Anchors anchors) => anchors.V;
    }
}
