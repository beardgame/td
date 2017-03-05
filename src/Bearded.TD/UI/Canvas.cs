namespace Bearded.TD.UI
{
    class Canvas
    {
        public IDimension X { get; }
        public IDimension Y { get; }

        public float XStart => X.Min;
        public float XEnd => X.Max;
        public float Width => X.Max - X.Min;

        public float YStart => Y.Min;
        public float YEnd => Y.Max;
        public float Height => Y.Max - Y.Min;

        public Canvas(IDimension x, IDimension y)
        {
            X = x;
            Y = y;
        }


        public static Canvas Within(Canvas parent, float top, float right, float bottom, float left)
        {
            return new Canvas(new FixedOffsetComponent(parent.X, left, right), new FixedOffsetComponent(parent.Y, top, bottom));
        }
    }
}