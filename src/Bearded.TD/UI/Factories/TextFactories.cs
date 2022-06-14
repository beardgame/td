using Bearded.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.UI.Text;

namespace Bearded.TD.UI.Factories;

static class TextFactories
{
    public static Label Header(string text, Vector2d? textAnchor = null, Color? color = null) =>
        Header(Binding.Create(text), textAnchor, color == null ? null : Binding.Create(color.Value));

    public static Label Header(Binding<string> text, Vector2d? textAnchor = null, Binding<Color>? color = null)
    {
        var label = new Label(text.Value)
        {
            Color = color?.Value ?? TextColor,
            FontSize = HeaderFontSize,
            TextAnchor = textAnchor ?? Controls.Label.TextAnchorLeft
        };
        text.SourceUpdated += newText => label.Text = newText;
        if (color != null)
        {
            color.SourceUpdated += newColor => label.Color = newColor;
        }

        return label;
    }

    public static Layouts.IColumnLayout AddHeader(
        this Layouts.IColumnLayout columnLayout, string text, Color? color = null)
    {
        return columnLayout.Add(Header(text, Controls.Label.TextAnchorLeft, color), HeaderLineHeight);
    }

    public static Layouts.IColumnLayout AddHeader(
        this Layouts.IColumnLayout columnLayout, Binding<string> text, Binding<Color>? color = null)
    {
        return columnLayout.Add(Header(text, Controls.Label.TextAnchorLeft, color), HeaderLineHeight);
    }

    public static Layouts.IRowLayout AddHeaderLeft(
        this Layouts.IRowLayout rowLayout, string text, double width, Color? color = null)
    {
        return rowLayout.AddLeft(
            Header(text, Controls.Label.TextAnchorLeft, color).WrapVerticallyCentered(HeaderLineHeight), width);
    }

    public static Layouts.IRowLayout AddColumnHeader(
        this Layouts.IRowLayout rowLayout, string text, double columnWidth, Color? color = null)
    {
        return rowLayout.AddLeft(
            Header(text, Controls.Label.TextAnchorCenter, color).WrapVerticallyCentered(HeaderLineHeight), columnWidth);
    }

    public static Layouts.IRowLayout AddColumnHeader(
        this Layouts.IRowLayout rowLayout, Binding<string> text, double columnWidth, Binding<Color>? color = null)
    {
        return rowLayout.AddLeft(
            Header(text, Controls.Label.TextAnchorCenter, color).WrapVerticallyCentered(HeaderLineHeight), columnWidth);
    }

    public static Label Label(string text, Vector2d? textAnchor = null, Color? color = null) => new(text)
    {
        Color = color ?? TextColor,
        FontSize = FontSize,
        TextAnchor = textAnchor ?? Controls.Label.TextAnchorCenter
    };

    public static Layouts.IColumnLayout AddLabel(
        this Layouts.IColumnLayout columnLayout, string text, Vector2d? textAnchor = null, Color? color = null)
    {
        return columnLayout.Add(Label(text, textAnchor, color), LineHeight);
    }

    public static Layouts.IRowLayout AddLabelLeft(
        this Layouts.IRowLayout rowLayout, string text, double width, Vector2d? textAnchor = null, Color? color = null)
    {
        return rowLayout.AddLeft(Label(text, textAnchor, color).WrapVerticallyCentered(LineHeight), width);
    }

    public static Control ValueLabel(
        string leftText,
        Binding<string> rightText,
        double? verticalAnchor = null,
        Color? leftColor = null,
        Binding<Color>? rightColor = null)
    {
        var valueLabel = Label(rightText.Value, new Vector2d(1, verticalAnchor ?? 0.5), rightColor?.Value);
        rightText.SourceUpdated += newValue => valueLabel.Text = newValue;
        if (rightColor != null)
        {
            rightColor.SourceUpdated += newValue => valueLabel.Color = newValue;
        }

        return new CompositeControl
        {
            Label(leftText, new Vector2d(0, verticalAnchor ?? 0.5), leftColor),
            valueLabel
        };
    }

    public static Layouts.IColumnLayout AddValueLabel(
        this Layouts.IColumnLayout columnLayout,
        string leftText,
        Binding<string> rightText,
        Color? leftColor = null,
        Binding<Color>? rightColor = null)
    {
        return columnLayout.Add(
            ValueLabel(leftText, rightText, leftColor: leftColor, rightColor: rightColor), LineHeight);
    }
}
