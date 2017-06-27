using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.UI.Components
{
    class TextInput : UIComponent
    {
        private const float fontSize = 14;

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
            if (input.Manager.IsKeyHit(Key.Enter))
            {
                Submitted?.Invoke(text);
                return;
            }
            if (input.Manager.IsKeyHit(Key.BackSpace) && cursorPosition > 0)
            {
                text = text.Substring(0, cursorPosition - 1) + text.Substring(cursorPosition);
                cursorPosition--;
            }
            if (input.Manager.IsKeyPressed(Key.Left) && cursorPosition > 0)
            {
                cursorPosition--;
            }
            if (input.Manager.IsKeyPressed(Key.Right) && cursorPosition < text.Length)
            {
                cursorPosition++;
            }
            if (input.Manager.IsKeyPressed(Key.Home))
            {
                cursorPosition = 0;
            }
            if (input.Manager.IsKeyPressed(Key.End))
            {
                cursorPosition = text.Length;
            }

            foreach (var c in input.State.PressedCharacters)
            {
                if ((c < '0' || c > '9') && (c < '@' || c > 'Z') && (c < 'a' || c > 'z') && !allowedChars.Contains(c))
                {
                    continue;
                }

                text = text.Substring(0, cursorPosition) + c + text.Substring(cursorPosition);
                cursorPosition++;
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            geometries.ConsoleFont.Height = fontSize;
            geometries.ConsoleFont.Color = Color.White;

            var height = Bounds.YStart + .5f * Bounds.Height;
            geometries.ConsoleFont.DrawString(new Vector2(Bounds.XStart, height), text, 0, .5f);

            var cursorXOffset = Bounds.XStart + geometries.ConsoleFont.StringWidth(text.Substring(0, cursorPosition));
            geometries.ConsoleFont.DrawString(new Vector2(cursorXOffset, height), cursorString, .5f, .5f);
        }
    }
}