using amulware.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;
using static amulware.Graphics.Color;

namespace Bearded.TD.Rendering.UI
{
    sealed class TextInputRenderer : IRenderer<TextInput>
    {
        private const string cursorString = "|";

        private readonly BoxRenderer boxRenderer;
        private readonly FontGeometry geometry;

        public TextInputRenderer(IndexedSurface<PrimitiveVertexData> primitives, IndexedSurface<UVColorVertexData> fontSurface, Font font)
        {
            boxRenderer = new BoxRenderer(primitives, White);

            geometry = new FontGeometry(fontSurface, font)
            {
                Color = White,
            };
        }

        public void Render(TextInput textInput)
        {
            boxRenderer.Render(textInput);

            var argb = White;
            if (!textInput.IsEnabled)
            {
                argb *= .5f;
            }

            geometry.Color = argb;
            geometry.Height = (float)textInput.FontSize;

            var topLeft = textInput.Frame.TopLeft;
            var midLeft = topLeft + Vector2d.UnitY * .5 * textInput.Frame.Size.Y;

            var textBeforeCursor = textInput.Text.Substring(0, textInput.CursorPosition);
            var stringWidthBeforeCursor = geometry.StringWidth(textBeforeCursor);

            geometry.DrawString((Vector2)midLeft, textInput.Text, alignY: .5f);

            if (textInput.IsFocused)
            {
                geometry.DrawString(
                    (Vector2) new Vector2d(midLeft.X + stringWidthBeforeCursor, midLeft.Y), cursorString, .5f, .5f);
            }
        }
    }
}
