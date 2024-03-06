using Bearded.Graphics;
using Bearded.Graphics.Text;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.Rendering.UI.Gradients;
using Bearded.TD.UI;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK.Mathematics;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.Rendering.UI;

sealed class TextInputRenderer(IShapeDrawer drawer, UIFonts fonts)
    : IRenderer<TextInput>
{
    private const string cursorString = "|";

    public void Render(TextInput textInput)
    {
        var textColor = Constants.UI.Colors.Get(textInput.IsEnabled ? ForeGroundColor.Text : ForeGroundColor.DisabledText);

        var frame = textInput.Frame;
        var topLeft = frame.TopLeft;
        var midLeft = topLeft + new Vector2d(4, textInput.Frame.Size.Y * .5);

        var backgroundColor = (textInput, textInput.MouseState) switch
        {
            ({ IsEnabled: false }, _) => BackgroundColor.InactiveElement,
            (_, { MouseIsDown: true }) => BackgroundColor.ActiveElement,
            (_, { MouseIsOver: true }) => BackgroundColor.Hover,
            ({ IsFocused: true }, _) => BackgroundColor.ActiveElement,
            _ => BackgroundColor.Element,
        };

        var colors = new ShapeColors(
            fill: Constants.UI.Colors.Get(backgroundColor),
            edge: Constants.UI.Colors.Get(ForeGroundColor.Edge),
            innerGlow: GradientParameters.SimpleGlow(Constants.UI.Colors.Get(BackgroundColor.Default))
            );
        var edges = new EdgeData(innerWidth: 1, innerGlow: 1.5f);
        drawer.Draw(Rectangle(frame.TopLeft, frame.Size, 2), colors, edges);

        var textDrawer = fonts.ForStyle(textInput.TextStyle);

        var textBeforeCursor = textInput.Text.Substring(0, textInput.CursorPosition);
        var stringOffset = textDrawer.StringWidth(textBeforeCursor, (float) textInput.FontSize);

        textDrawer.DrawLine(
            xyz: ((Vector2) midLeft).WithZ(),
            text: textInput.Text,
            fontHeight: (float) textInput.FontSize,
            parameters: textColor,
            alignVertical: 0.5f
        );

        if (textInput.IsFocused)
        {
            textDrawer.DrawLine(
                xyz: ((Vector2) midLeft).WithZ() + stringOffset,
                text: cursorString,
                fontHeight: (float) textInput.FontSize,
                alignHorizontal: 0.5f,
                alignVertical: 0.5f,
                parameters: textColor
            );
        }
    }
}
