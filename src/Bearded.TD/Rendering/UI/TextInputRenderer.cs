using Bearded.Graphics;
using Bearded.Graphics.Text;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK.Mathematics;
using static Bearded.Graphics.Color;

namespace Bearded.TD.Rendering.UI;

sealed class TextInputRenderer(IShapeDrawer shapeDrawer, TextDrawerWithDefaults<Color> textDrawer)
    : IRenderer<TextInput>
{
    private const string cursorString = "|";

    private readonly BoxRenderer boxRenderer = new(shapeDrawer, White);
    private readonly TextDrawerWithDefaults<Color> textDrawer = textDrawer.With(alignVertical: .5f);

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
        var stringOffset = textDrawer.StringWidth(textBeforeCursor, (float) textInput.FontSize);

        textDrawer.DrawLine(
            xyz: ((Vector2) midLeft).WithZ(),
            text: textInput.Text,
            fontHeight: (float) textInput.FontSize,
            parameters: argb
        );

        if (textInput.IsFocused)
        {
            textDrawer.DrawLine(
                xyz: ((Vector2) midLeft).WithZ() + stringOffset,
                text: cursorString,
                fontHeight: (float) textInput.FontSize,
                alignHorizontal: .5f,
                parameters: argb
            );
        }
    }
}
