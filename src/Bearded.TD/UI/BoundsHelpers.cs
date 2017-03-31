namespace Bearded.TD.UI
{
    partial class Bounds
    {
        public static Bounds Within(Bounds parent, float top, float right, float bottom, float left)
        {
            return new Bounds(new FixedOffsetDimension(parent.X, left, right), new FixedOffsetDimension(parent.Y, top, bottom));
        }
    }
}