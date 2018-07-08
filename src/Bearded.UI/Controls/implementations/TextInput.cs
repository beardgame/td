using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using static OpenTK.Input.Key;
using MouseButtonEventArgs = Bearded.UI.EventArgs.MouseButtonEventArgs;

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

        public override void MouseButtonHit(MouseButtonEventArgs eventArgs)
        {
            eventArgs.Handled = TryFocus();
        }

        public override void KeyHit(KeyEventArgs eventArgs)
        {
            eventArgs.Handled = tryHandleKeyHit(eventArgs);
        }

        private bool tryHandleKeyHit(KeyEventArgs eventArgs)
        {
            switch (eventArgs.Key)
            {
                case Left:
                    cursorPosition--;
                    ensureValidCursorPosition();
                    break;
                case Right:
                    cursorPosition++;
                    ensureValidCursorPosition();
                    break;
                case BackSpace:
                    RemoveCharacterBeforeCursorIfPossible();
                    break;
                case Delete:
                    RemoveCharacterAfterCursorIfPossible();
                    break;
                case Home:
                    MoveCursorToBeginning();
                    break;
                case End:
                    MoveCursorToEnd();
                    break;
                default:
                    return false;
            }
            return true;
        }

        public override void CharacterTyped(CharEventArgs eventArgs)
        {
            InsertTextAtCursor(eventArgs.Character.ToString());
            eventArgs.Handled = true;
        }
        
        public void MoveCursorToEnd()
        {
            cursorPosition = text.Length;
        }

        public void MoveCursorToBeginning()
        {
            cursorPosition = 0;
        }

        public void RemoveCharacterBeforeCursorIfPossible()
        {
            if (cursorPosition == 0)
                return;

            cursorPosition--;
            text = text.Remove(cursorPosition, 1);
            
            onTextChanged();
        }

        public void RemoveCharacterAfterCursorIfPossible()
        {
            if (cursorPosition == text.Length)
                return;
            
            text = text.Remove(cursorPosition, 1);

            onTextChanged();
        }

        public void InsertTextAtCursor(string textToInsert)
        {
            if (string.IsNullOrEmpty(textToInsert))
                return;

            text = text.Insert(cursorPosition, textToInsert);
            cursorPosition += textToInsert.Length;

            onTextChanged();
        }

        public void Clear()
        {
            MoveCursorToBeginning();
            Text = "";
        }

        private void onTextChanged()
        {
            TextChanged?.Invoke();
        }

        private void ensureValidCursorPosition()
        {
            cursorPosition = clampedCursorPosition(cursorPosition);
        }

        private int clampedCursorPosition(int candidate) => candidate.Clamped(0, text.Length);

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
