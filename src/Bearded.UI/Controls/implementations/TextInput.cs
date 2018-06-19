using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;
using Bearded.Utilities;

namespace Bearded.UI.Controls
{
    public class TextInput : Control
    {
        public event VoidEventHandler TextChanged;

        private string text = "";
        private int cursorPosition;

        public string Text
        {
            get => text;
            set
            {
                var newText = value ?? "";
                if (text == newText)
                    return;
                text = newText;
                ensureValidCursorPosition();
                onTextChanged();
            }
        }

        public int CursorPosition
        {
            get => cursorPosition;
            set
            {
                cursorPosition = value;
                ensureValidCursorPosition();
            }
        }

        public TextInput()
        {
            CanBeFocused = true;
        }

        private void ensureValidCursorPosition()
        {
            cursorPosition = clampedCursorPosition(cursorPosition);
        }

        private int clampedCursorPosition(int candidate) => candidate.Clamped(0, text.Length);

        private void onTextChanged()
        {
            TextChanged?.Invoke();
        }

        public void MoveCursorToEnd()
        {
            CursorPosition = text.Length;
        }

        public void InsertTextAtCursor(string textToInsert)
        {
            if (string.IsNullOrEmpty(textToInsert))
                return;

            text = text.Insert(cursorPosition, textToInsert);

            cursorPosition += textToInsert.Length;
        }

        public override void MouseButtonHit(MouseButtonEventArgs eventArgs)
        {
            eventArgs.Handled = TryFocus();
        }

        public override void KeyHit(KeyEventArgs eventArgs)
        {
            base.KeyHit(eventArgs);
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
