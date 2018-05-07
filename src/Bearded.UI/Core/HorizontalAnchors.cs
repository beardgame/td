namespace Bearded.UI
{
    public struct HorizontalAnchors
    {
        private readonly Anchors anchors;

        public Anchor Left => anchors.Start;
        public Anchor Right => anchors.End;

        public HorizontalAnchors(Anchors anchors)
        {
            this.anchors = anchors;
        }

        public static implicit operator Anchors(HorizontalAnchors anchors) => anchors.anchors;
    }
}
