namespace Bearded.TD.UI
{
    class AnchoredFixedSizeDimension : IDimension
    {
        private readonly IDimension parent;
        private readonly float anchor;
        private readonly float size;
        private readonly float offset;

        public AnchoredFixedSizeDimension(IDimension parent, float anchor, float size, float offset = 0)
        {
            this.parent = parent;
            this.anchor = anchor;
            this.size = size;
            this.offset = offset;
        }

        private float anchorAbsolute => parent.Min + anchor * (parent.Max - parent.Min);

        public float Min => anchorAbsolute - anchor * size + offset;
        public float Max => anchorAbsolute + (1 - anchor) * size + offset;
    }
}
