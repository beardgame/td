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

        public static Bounds Box(Bounds parent, Vector2 centerOffset, Vector2 size)
        {
            return new Bounds(
                new FixedSizeAndOffsetDimension(parent.X, size.X, centerOffset.X),
                new FixedSizeAndOffsetDimension(parent.Y, size.Y, centerOffset.Y));
        }
    }

    static class BoundsExtensions
    {
        public static bool Contains(this Bounds bounds, Vector2 point)
            => point.X >= bounds.XStart && point.X <= bounds.XEnd && point.Y >= bounds.YStart && point.Y <= bounds.YEnd;

        public static float CenterX(this Bounds bounds) => .5f * (bounds.XStart + bounds.XEnd);
        public static float CenterY(this Bounds bounds) => .5f * (bounds.YStart + bounds.YEnd);
    }
}