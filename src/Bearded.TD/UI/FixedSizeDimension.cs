namespace Bearded.TD.UI
{
    class FixedSizeDimension : IDimension
    {
        private readonly IDimension parent;
        private readonly float size;
        private readonly float anchor;
        private readonly float pivot;

        public FixedSizeDimension(IDimension parent, float size, float anchor = 0, float pivot = 0)
        {
            this.parent = parent;
            this.size = size;
            this.anchor = anchor;
            this.pivot = pivot;
        }

        private float anchorAbsolute => parent.Min + anchor * (parent.Max - parent.Min);
        private float pivotAbsolute => size * pivot;

        public float Min => anchorAbsolute - pivotAbsolute;
        public float Max => Min + size;
    }
}