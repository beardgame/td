using Bearded.Graphics;
using Bearded.Graphics.Text;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK.Mathematics;
using static Bearded.Graphics.Color;

namespace Bearded.TD.Rendering.UI;

sealed class AutoCompletingTextInputRenderer(IShapeDrawer shapeDrawer, TextDrawerWithDefaults<Color> textDrawer)
    : IRenderer<AutoCompletingTextInput>
{
    private readonly TextInputRenderer internalRenderer = new(shapeDrawer, textDrawer);

    public void Render(AutoCompletingTextInput textInput)
    {
        internalRenderer.Render(textInput);

        if (!textInput.IsEnabled)
        {
            return;
        }

        var argb = White * .5f;

        var stringOffset = textDrawer.StringWidth(textInput.Text, (float) textInput.FontSize);

        if (textInput.Text.Length >= textInput.AutoCompletionText.Length)
            return;

        var str = textInput.AutoCompletionText[textInput.Text.Length..];

        var midLeft = textInput.Frame.TopLeft + new Vector2d(4, textInput.Frame.Size.Y * .5);
        textDrawer.DrawLine(
            xyz: ((Vector2) midLeft).WithZ() + stringOffset,
            text: str,
            fontHeight: (float) textInput.FontSize,
            alignVertical: 0.5f,
            parameters: argb
        );
    }
}
