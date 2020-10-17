using amulware.Graphics;
using amulware.Graphics.Shapes;
using amulware.Graphics.Text;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenToolkit.Mathematics;
using static amulware.Graphics.Color;

namespace Bearded.TD.Rendering.UI
{
    sealed class TextInputRenderer : IRenderer<TextInput>
    {
        private const string cursorString = "|";

        private readonly BoxRenderer boxRenderer;
        private readonly TextDrawerWithDefaults<Color> textDrawer;

        public TextInputRenderer(IShapeDrawer2<Color> shapeDrawer, TextDrawerWithDefaults<Color> textDrawer)
        {
            boxRenderer = new BoxRenderer(shapeDrawer, White);
            this.textDrawer = textDrawer.With(alignVertical: .5f);
        }

        public void Render(TextInput textInput)
        {
            boxRenderer.Render(textInput);

            var argb = White;
            if (!textInput.IsEnabled)
            {
                argb *= .5f;
            }

            var topLeft = textInput.Frame.TopLeft;
            var midLeft = topLeft + Vector2d.UnitY * .5 * textInput.Frame.Size.Y;

            var textBeforeCursor = textInput.Text.Substring(0, textInput.CursorPosition);
            var stringWidthBeforeCursor = geometry.StringWidth(textBeforeCursor);

            textDrawer.DrawLine(
                xyz: ((Vector2) midLeft).WithZ(),
                text: textInput.Text,
                fontHeight: (float) textInput.FontSize,
                parameters: argb
            );

            if (textInput.IsFocused)
            {
                var cursorPos = new Vector2d(midLeft.X + stringWidthBeforeCursor, midLeft.Y);

                textDrawer.DrawLine(
                    xyz: ((Vector2) cursorPos).WithZ(),
                    text: cursorString,
                    fontHeight: (float) textInput.FontSize,
                    alignHorizontal: .5f,
                    parameters: argb
                );
            }
        }
    }
}
