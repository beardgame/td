using OpenTK;

namespace Bearded.TD.UI
{
    partial class Bounds
    {
        public static Bounds Within(Bounds parent, float top, float right, float bottom, float left)
        {
            return new Bounds(
                new FixedOffsetDimension(parent.X, left, right),
                new FixedOffsetDimension(parent.Y, top, bottom));
        }

        public static Bounds AnchoredBox(Bounds parent, float xAnchor, float yAnchor, Vector2 size)
            => AnchoredBox(parent, xAnchor, yAnchor, size, Vector2.Zero);

        public static Bounds AnchoredBox(Bounds parent, float xAnchor, float yAnchor, Vector2 size, Vector2 offset)
        {
            return new Bounds(
                new AnchoredFixedSizeDimension(parent.X, xAnchor, size.X, offset.X),
                new AnchoredFixedSizeDimension(parent.Y, yAnchor, size.Y, offset.Y));
        }
    }

    static class BoundsAnchor
    {
        public static float Start => 0;
        public static float Center => .5f;
        public static float End => 1;
    }

    static class BoundsExtensions
    {
        public static bool Contains(this Bounds bounds, Vector2 point)
            => point.X >= bounds.XStart && point.X <= bounds.XEnd && point.Y >= bounds.YStart && point.Y <= bounds.YEnd;

        public static float CenterX(this Bounds bounds) => .5f * (bounds.XStart + bounds.XEnd);
        public static float CenterY(this Bounds bounds) => .5f * (bounds.YStart + bounds.YEnd);
        public static Vector2 Start(this Bounds bounds) => new Vector2(bounds.XStart, bounds.YStart);
        public static Vector2 End(this Bounds bounds) => new Vector2(bounds.XEnd, bounds.YEnd);
        public static Vector2 Size(this Bounds bounds) => new Vector2(bounds.Width, bounds.Height);
    }
}