using OpenTK;

namespace Bearded.UI.EventArgs
{
    public sealed class MouseScrollEventArgs : MouseEventArgs
    {
        public int DeltaScroll { get; }
        public float DeltaScrollF { get; }

        public MouseScrollEventArgs(Vector2d mousePosition, int deltaScroll, float deltaScrollF)
            : base(mousePosition)
        {
            DeltaScroll = deltaScroll;
            DeltaScrollF = deltaScrollF;
        }
    }
}
