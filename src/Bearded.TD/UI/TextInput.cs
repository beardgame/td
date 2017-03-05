using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.Utilities.Input;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.UI
{
    class TextInput
    {
        private const float fontSize = 14;

        private static readonly HashSet<char> allowedChars = new HashSet<char> {' ', '-', '_', '.', '+'};
        private const string cursorString = "|";

        private readonly Canvas canvas;

        public string Text { get; private set; } = "";
        private int cursorPosition = 0;

        public TextInput(Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void HandleInput(InputState inputState)
        {
            if (InputManager.IsKeyHit(Key.BackSpace) && cursorPosition > 0)
            {
                Text = Text.Substring(0, cursorPosition - 1) + Text.Substring(cursorPosition);
                cursorPosition--;
            }
            if (InputManager.IsKeyPressed(Key.Left) && cursorPosition > 0)
            {
                cursorPosition--;
            }
            if (InputManager.IsKeyPressed(Key.Right) && cursorPosition < Text.Length)
            {
                cursorPosition++;
            }
            if (InputManager.IsKeyPressed(Key.Home))
            {
                cursorPosition = 0;
            }
            if (InputManager.IsKeyPressed(Key.End))
            {
                cursorPosition = Text.Length;
            }

            foreach (var c in inputState.PressedCharacters)
            {
                if ((c < '0' || c > '9') && (c < '@' || c > 'Z') && (c < 'a' || c > 'z') && !allowedChars.Contains(c))
                {
                    continue;
                }

                Text = Text.Substring(0, cursorPosition) + c + Text.Substring(cursorPosition);
                cursorPosition++;
            }
        }

        public void Draw(GeometryManager geometries)
        {
            geometries.ConsoleFont.Height = fontSize;
            geometries.ConsoleFont.Color = Color.White;

            var height = canvas.YStart + .5f * canvas.Height;
            geometries.ConsoleFont.DrawString(new Vector2(canvas.XStart, height), Text, 0, .5f);

            var cursorXOffset = canvas.XStart + geometries.ConsoleFont.StringWidth(Text.Substring(0, cursorPosition));
            geometries.ConsoleFont.DrawString(new Vector2(cursorXOffset, height), cursorString, .5f, .5f);
        }
    }
}