using amulware.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;
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

            geometry.Height = (float)textInput.FontSize;
            
            var topLeft = textInput.Frame.TopLeft;

            var textBeforeCursor = textInput.Text.Substring(0, textInput.CursorPosition);
            var stringWidthBeforeCursor = geometry.StringWidth(textBeforeCursor);

            geometry.DrawString((Vector2)topLeft, textInput.Text);

            geometry.DrawString(
                (Vector2)new Vector2d(topLeft.X + stringWidthBeforeCursor, topLeft.Y + textInput.FontSize * 0.5f),
                cursorString, .5f, .5f
                );
        }
    }
}
