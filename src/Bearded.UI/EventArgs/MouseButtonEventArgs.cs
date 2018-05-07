using OpenTK;
using OpenTK.Input;

namespace Bearded.UI.EventArgs
{
    public class MouseButtonEventArgs : MouseEventArgs
    {
        public MouseButton MouseButton { get; }

        public MouseButtonEventArgs(Vector2d mousePosition, MouseButton mouseButton) : base(mousePosition)
        {
            MouseButton = mouseButton;
        }
    }
}
