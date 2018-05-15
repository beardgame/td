using OpenTK;

namespace Bearded.UI.EventArgs
{
    public class MouseEventArgs : RoutedEventArgs
    {
        public Vector2d MousePosition { get; }

        public MouseEventArgs(Vector2d mousePosition)
        {
            MousePosition = mousePosition;
        }
    }
}
