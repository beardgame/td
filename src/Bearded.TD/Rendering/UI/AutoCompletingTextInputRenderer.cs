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
    sealed class AutoCompletingTextInputRenderer : IRenderer<AutoCompletingTextInput>
    {
        private readonly TextInputRenderer internalRenderer;
        private readonly TextDrawerWithDefaults<Color> textDrawer;

        public AutoCompletingTextInputRenderer(
            IShapeDrawer2<Color> shapeDrawer, TextDrawerWithDefaults<Color> textDrawer)
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

            var stringOffset = textDrawer.StringWidth(textInput.Text, (float) textInput.FontSize);

            var str = textInput.AutoCompletionText.Substring(textInput.Text.Length);

            textDrawer.DrawLine(
                xyz: ((Vector2) textInput.Frame.TopLeft).WithZ() + stringOffset,
                text: str,
                fontHeight: (float) textInput.FontSize,
                alignVertical: 0,
                parameters: argb
            );
        }
    }
}
