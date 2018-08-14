using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK.Input;
using MouseButtonEventArgs = Bearded.UI.EventArgs.MouseButtonEventArgs;

namespace Bearded.UI.Controls
{
    public class Button : CompositeControl
    {
        public event VoidEventHandler Clicked;

        public bool IsEnabled { get; set; } = true;

        public Button()
        {
            CanBeFocused = true;
        }

        public override void MouseButtonReleased(MouseButtonEventArgs eventArgs)
        {
            base.MouseButtonReleased(eventArgs);
            if (eventArgs.MouseButton == MouseButton.Left && IsEnabled)
            {
                Clicked?.Invoke();
            }
            eventArgs.Handled = true;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
