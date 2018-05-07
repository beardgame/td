using OpenTK;

namespace Bearded.UI
{
    public struct Frame
    {
        public readonly Vector2d TopLeft;
        public readonly Vector2d Size;

        public Frame(Vector2d topLeft, Vector2d size)
        {
            TopLeft = topLeft;
            Size = size;
        }
    }
}
