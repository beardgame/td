namespace Bearded.TD.UI
{
    class FixedSizeAndOffsetDimension : IDimension
    {
        private readonly IDimension parent;
        private readonly float size;
        private readonly float offset;
        private readonly float pivot;

        public FixedSizeAndOffsetDimension(IDimension parent, float size, float offset, float pivot = .5f)
        {
            this.parent = parent;
            this.size = size;
            this.offset = offset;
            this.pivot = pivot;
        }

        private float anchorAbsolute => parent.Min + offset;
        private float pivotAbsolute => size * pivot;

        public float Min => anchorAbsolute - pivotAbsolute;
        public float Max => Min + size;
    }
}
