using amulware.Graphics;
using amulware.Graphics.Shapes;
using amulware.Graphics.Text;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenToolkit.Mathematics;
using static amulware.Graphics.Color;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;

namespace Bearded.TD.Rendering.UI
{
    sealed class AutoCompletingTextInputRenderer : IRenderer<AutoCompletingTextInput>
    {
        private readonly TextInputRenderer internalRenderer;
        private readonly TextDrawerWithDefaults<Color> textDrawer;

        public AutoCompletingTextInputRenderer(
            ShapeDrawer2<ColorVertexData, Color> shapeDrawer, TextDrawerWithDefaults<Color> textDrawer)
        {
            internalRenderer = new TextInputRenderer(shapeDrawer, textDrawer);
            this.textDrawer = textDrawer;
        }

        public void Render(AutoCompletingTextInput textInput)
        {
            internalRenderer.Render(textInput);

            if (!textInput.IsEnabled)
            {
                return;
            }

            var argb = White * .5f;

            var textWidth = geometry.StringWidth(textInput.Text);

            var topLeft = textInput.Frame.TopLeft + textWidth * Vector2d.UnitX;
            var str = textInput.AutoCompletionText.Substring(textInput.Text.Length);

            textDrawer.DrawLine(
                xyz: ((Vector2) topLeft).WithZ(),
                text: str,
                fontHeight: (float) textInput.FontSize,
                alignVertical: 0,
                parameters: argb
            );
        }
    }
}
