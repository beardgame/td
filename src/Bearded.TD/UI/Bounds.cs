namespace Bearded.TD.UI
{
    partial class Bounds
    {
        public IDimension X { get; }
        public IDimension Y { get; }

        public float XStart => X.Min;
        public float XEnd => X.Max;
        public float Width => X.Max - X.Min;

        public float YStart => Y.Min;
        public float YEnd => Y.Max;
        public float Height => Y.Max - Y.Min;

        public Bounds(IDimension x, IDimension y)
        {
            X = x;
            Y = y;
        }
    }
}