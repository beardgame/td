using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;

namespace Bearded.UI.Controls
{
    public class TextInput : Control
    {
        private string text = "";
        private int cursorPosition;

        public string Text
        {
            get => text;
            set => text = value ?? "";
        }

        public int CursorPosition
        {
            get => cursorPosition;
            set => cursorPosition = value;
        }

        public TextInput()
        {
            CanBeFocused = true;
        }

        public void MoveCursorToEnd()
        {
            cursorPosition = text.Length;
        }

        public override void MouseButtonHit(MouseButtonEventArgs eventArgs)
        {
            eventArgs.Handled = TryFocus();
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
