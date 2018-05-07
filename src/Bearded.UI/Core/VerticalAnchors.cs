namespace Bearded.UI
{
    public struct VerticalAnchors
    {
        private readonly Anchors anchors;

        public Anchor Top => anchors.Start;
        public Anchor Bottom => anchors.End;

        public VerticalAnchors(Anchors anchors)
        {
            this.anchors = anchors;
        }

        public static implicit operator Anchors(VerticalAnchors anchors) => anchors.anchors;
    }
}
