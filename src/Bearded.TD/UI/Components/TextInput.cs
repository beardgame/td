using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.UI.Components
{
    class TextInput : FocusableUIComponent
    {
        private static readonly HashSet<char> allowedChars = new HashSet<char> {' ', '-', '_', '.', '+', '"'};
        private const string cursorString = "|";

        private int cursorPosition;
        private string text = "";

        public string Text
        {
            get => text;
            set
            {
                text = value ?? "";
                cursorPosition = text.Length;
            }
        }

        public event GenericEventHandler<string> Submitted; 

        public TextInput(Bounds bounds) : base(bounds)
        {
        }

        public override void HandleInput(InputContext input)
        {
            if (!IsFocused)
            {
                return;
            }
            if (input.State.ForKey(Key.Enter).Hit)
            {
                Submitted?.Invoke(text);
                input.State.Keyboard.Capture();
                return;
            }
            if (input.State.ForKey(Key.BackSpace).Hit && cursorPosition > 0)
            {
                text = text.Substring(0, cursorPosition - 1) + text.Substring(cursorPosition);
                cursorPosition--;
            }
            if (input.State.ForKey(Key.Left).Hit && cursorPosition > 0)
            {
                cursorPosition--;
            }
            if (input.State.ForKey(Key.Right).Hit && cursorPosition < text.Length)
            {
                cursorPosition++;
            }
            if (input.State.ForKey(Key.Home).Hit)
            {
                cursorPosition = 0;
            }
            if (input.State.ForKey(Key.End).Hit)
            {
                cursorPosition = text.Length;
            }

            foreach (var c in input.State.Keyboard.PressedCharacters)
            {
                if ((c < '0' || c > '9') && (c < '@' || c > 'Z') && (c < 'a' || c > 'z') && !allowedChars.Contains(c))
                {
                    continue;
                }

                text = text.Substring(0, cursorPosition) + c + text.Substring(cursorPosition);
                cursorPosition++;
            }
            input.State.Keyboard.Capture();
        }

        public override void Draw(GeometryManager geometries)
        {
            geometries.ConsoleFont.Height = Constants.UI.FontSize;
            geometries.ConsoleFont.Color = Color.White;

            var height = Bounds.YStart + .5f * Bounds.Height;
            geometries.ConsoleFont.DrawString(new Vector2(Bounds.XStart, height), text, 0, .5f);

            geometries.ConsoleBackground.Color = IsFocused ? Color.HotPink : Color.WhiteSmoke;
            geometries.ConsoleBackground.LineWidth = 1;
            geometries.ConsoleBackground.DrawLine(new Vector2(Bounds.XStart, Bounds.YEnd), new Vector2(Bounds.XEnd, Bounds.YEnd));

            if (!IsFocused)
            {
                return;
            }

            var cursorXOffset = Bounds.XStart + geometries.ConsoleFont.StringWidth(text.Substring(0, cursorPosition));
            geometries.ConsoleFont.DrawString(new Vector2(cursorXOffset, height), cursorString, .5f, .5f);
        }
    }
}