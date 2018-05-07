namespace Bearded.UI
{
    public struct Anchors
    {
        public readonly Anchor Start;
        public readonly Anchor End;

        public Anchors(Anchor start, Anchor end)
        {
            Start = start;
            End = end;
        }

        public HorizontalAnchors H => new HorizontalAnchors(this);
        public VerticalAnchors V => new VerticalAnchors(this);

        public static explicit operator HorizontalAnchors(Anchors anchors) => anchors.H;
        public static explicit operator VerticalAnchors(Anchors anchors) => anchors.V;
    }
}
