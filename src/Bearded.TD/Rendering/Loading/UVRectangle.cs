using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Loading
{
    readonly struct UVRectangle
    {
        public float Left { get; }
        public float Right { get; }
        public float Top { get; }
        public float Bottom { get; }
        public Vector2 TopLeft => new Vector2(Left, Top);
        public Vector2 TopRight => new Vector2(Right, Top);
        public Vector2 BottomLeft => new Vector2(Left, Bottom);
        public Vector2 BottomRight => new Vector2(Right, Bottom);

        public UVRectangle(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public void Deconstruct(out float left, out float right, out float top, out float bottom)
        {
            left = Left;
            right = Right;
            top = Top;
            bottom = Bottom;
        }
    }
}
