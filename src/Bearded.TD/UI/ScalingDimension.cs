namespace Bearded.TD.UI
{
    class ScalingDimension : IDimension
    {
        private readonly IDimension parent;
        private readonly float fraction;
        private readonly float offset;

        public ScalingDimension(IDimension parent, float fraction = 1, float offset = 0)
        {
            this.parent = parent;
            this.fraction = fraction;
            this.offset = offset;
        }

        public float Min => offset * (parent.Max - parent.Min) + parent.Min * fraction;
        public float Max => offset * (parent.Max - parent.Min) + parent.Max * fraction;
    }
}