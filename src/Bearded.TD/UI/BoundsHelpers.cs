using System.Net;
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

        public static Bounds AnchoredBox(Bounds parent, Vector2 anchor, Vector2 size)
            => AnchoredBox(parent, anchor, size, Vector2.Zero);

        public static Bounds AnchoredBox(Bounds parent, Vector2 anchor, Vector2 size, Vector2 offset)
            => new Bounds(
                    new AnchoredFixedSizeDimension(parent.X, anchor.X, size.X, offset.X),
                    new AnchoredFixedSizeDimension(parent.Y, anchor.Y, size.Y, offset.Y));
    }

    static class BoundsConstants
    {
        public static float Start => 0;
        public static float Center => .5f;
        public static float Middle => .5f;
        public static float End => 1;

        public static float Left => Start;
        public static float Right => End;
        public static float Top => Start;
        public static float Bottom => End;

        public static Vector2 TopLeft => new Vector2(Left, Top);
        public static Vector2 TopCenter => new Vector2(Center, Top);
        public static Vector2 TopRight => new Vector2(Right, Top);
        public static Vector2 MiddleLeft => new Vector2(Left, Middle);
        public static Vector2 MiddleCenter => new Vector2(Center, Middle);
        public static Vector2 MiddleRight => new Vector2(Right, Middle);
        public static Vector2 BottomLeft => new Vector2(Left, Bottom);
        public static Vector2 BottomCenter => new Vector2(Center, Bottom);
        public static Vector2 BottomRight => new Vector2(Right, Bottom);

        public static Vector2 Size(float w, float h) => new Vector2(w, h);
        public static Vector2 Offset(float xOff, float yOff) => new Vector2(xOff, yOff);
        public static Vector2 OffsetFrom(Vector2 anchor, float xOff, float yOff)
            => new Vector2(xOff * (1 - 2 * anchor.X), yOff * (1 - 2 * anchor.Y));
    }

    static class BoundsExtensions
    {
        public static bool Contains(this Bounds bounds, Vector2 point)
            => point.X >= bounds.Left && point.X <= bounds.Right && point.Y >= bounds.Top && point.Y <= bounds.Bottom;

        public static float CenterX(this Bounds bounds) => .5f * (bounds.Left + bounds.Right);
        public static float CenterY(this Bounds bounds) => .5f * (bounds.Top + bounds.Bottom);
        public static Vector2 TopLeft(this Bounds bounds) => new Vector2(bounds.Left, bounds.Top);
        public static Vector2 BottomRight(this Bounds bounds) => new Vector2(bounds.Right, bounds.Bottom);
        public static Vector2 Size(this Bounds bounds) => new Vector2(bounds.Width, bounds.Height);
    }
}