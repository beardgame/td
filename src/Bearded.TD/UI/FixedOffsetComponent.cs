namespace Bearded.TD.UI
{
    class FixedOffsetComponent : IDimension
    {
        private readonly IDimension parent;
        private readonly float offsetStart;
        private readonly float offsetEnd;

        public FixedOffsetComponent(IDimension parent, float offsetStart, float offsetEnd)
        {
            this.parent = parent;
            this.offsetStart = offsetStart;
            this.offsetEnd = offsetEnd;
        }

        public float Min => parent.Min + offsetStart;
        public float Max => parent.Max - offsetEnd;
    }
}