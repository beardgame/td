namespace Bearded.TD.UI
{
    partial class Bounds
    {
        public IDimension X { get; }
        public IDimension Y { get; }

        public float Left => X.Min;
        public float Right => X.Max;
        public float Width => X.Max - X.Min;

        public float Top => Y.Min;
        public float Bottom => Y.Max;
        public float Height => Y.Max - Y.Min;

        public Bounds(IDimension x, IDimension y)
        {
            X = x;
            Y = y;
        }
    }
}