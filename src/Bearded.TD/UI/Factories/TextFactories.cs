using Bearded.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.UI.Text;

namespace Bearded.TD.UI.Factories;

static class TextFactories
{
    public static Control Header(
        IReadonlyBinding<string> text, Vector2d? textAnchor = null, IReadonlyBinding<Color>? color = null,
        ShapeColor? backgroundColor = null)
    {
        var builder = new HeaderBuilder().WithText(text);

        if (textAnchor != null)
            builder.WithTextAnchor(textAnchor.Value);

        if (color != null)
            builder.WithColor(color.Value);

        if (backgroundColor != null)
            builder.WithBackground(Background.From(backgroundColor.Value));

        return builder.Build();
    }

    public static Control Header(
        string text, Vector2d? textAnchor = null, Color? color = null, ShapeColor? backgroundColor = null)
    {
        var builder = new HeaderBuilder().WithText(text);

        if (textAnchor != null)
            builder.WithTextAnchor(textAnchor.Value);

        if (color != null)
            builder.WithColor(color.Value);

        if (backgroundColor != null)
            builder.WithBackground(Background.From(backgroundColor.Value));

        return builder.Build();
    }

    public sealed class HeaderBuilder
    {
        private object? text;
        private Vector2d? textAnchor;
        private object? color;
        private Background background;

        public HeaderBuilder WithText(string text)
        {
            this.text = text;
            return this;
        }

        public HeaderBuilder WithText(IReadonlyBinding<string> text)
        {
            this.text = text;
            return this;
        }

        public HeaderBuilder WithTextAnchor(Vector2d textAnchor)
        {
            this.textAnchor = textAnchor;
            return this;
        }

        public HeaderBuilder WithColor(Color color)
        {
            this.color = color;
            return this;
        }

        public HeaderBuilder WithColor(IReadonlyBinding<Color> color)
        {
            this.color = color;
            return this;
        }

        public HeaderBuilder WithBackground(Background background)
        {
            this.background = background;
            return this;
        }

        public Control Build()
        {
            var label = new Label
            {
                FontSize = HeaderFontSize,
                TextAnchor = textAnchor ?? Controls.Label.TextAnchorLeft,
                TextStyle = TextStyle.Header,
            };
            label.Text = text switch
            {
                string s => s,
                IReadonlyBinding<string> b => b.ReturnCurrentAndKeepUpdated(s => label.Text = s),
                _ => "",
            };
            label.Color = color switch
            {
                Color c => c,
                IReadonlyBinding<Color> b => b.ReturnCurrentAndKeepUpdated(c => label.Color = c),
                _ => TextColor,
            };

            var backgroundControl = background.ToControl();

            if (backgroundControl == null)
                return label;

            return new CompositeControl
            {
                backgroundControl,
                label.Anchor(a => a.MarginAllSides(LabelInBackgroundMargin)),
            };
        }
    }

    public static Layouts.IColumnLayout AddSeparator(this Layouts.IColumnLayout columnLayout)
    {
        return columnLayout.Add(
            new ComplexBox { Components = Constants.UI.Layout.HorizontalSeparator },
            Constants.UI.Layout.HorizontalSeparatorHeight
        );
    }

    public static Layouts.IColumnLayout AddHeader(
        this Layouts.IColumnLayout columnLayout, string text, Color? color = null)
    {
        return columnLayout.Add(Header(text, Controls.Label.TextAnchorLeft, color), HeaderLineHeight);
    }

    public static Layouts.IColumnLayout AddHeader(
        this Layouts.IColumnLayout columnLayout, IReadonlyBinding<string> text, IReadonlyBinding<Color>? color = null)
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
        this Layouts.IRowLayout rowLayout,
        IReadonlyBinding<string> text,
        double columnWidth,
        IReadonlyBinding<Color>? color = null)
    {
        return rowLayout.AddLeft(
            Header(text, Controls.Label.TextAnchorCenter, color).WrapVerticallyCentered(HeaderLineHeight), columnWidth);
    }

    public static Label Label(
        IReadonlyBinding<string> text, Vector2d? textAnchor = null, IReadonlyBinding<Color>? color = null)
    {
        var label = Label(text.Value, textAnchor, color?.Value);
        text.SourceUpdated += newText => label.Text = newText;
        text.ControlUpdated += newText => label.Text = newText;
        if (color != null)
        {
            color.SourceUpdated += newColor => label.Color = newColor;
            color.ControlUpdated += newColor => label.Color = newColor;
        }

        return label;
    }

    public static Label Label(string text, Vector2d? textAnchor = null, Color? color = null) => new()
    {
        Text = text,
        Color = color ?? TextColor,
        FontSize = FontSize,
        TextAnchor = textAnchor ?? Controls.Label.TextAnchorCenter
    };

    public static Layouts.IColumnLayout AddLabel(
        this Layouts.IColumnLayout columnLayout,
        IReadonlyBinding<string> text, Vector2d? textAnchor = null, IReadonlyBinding<Color>? color = null)
    {
        return columnLayout.Add(Label(text, textAnchor, color), LineHeight);
    }

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
        IReadonlyBinding<string> rightText,
        double? verticalAnchor = null,
        Color? leftColor = null,
        IReadonlyBinding<Color>? rightColor = null)
    {
        return new CompositeControl
        {
            Label(leftText, new Vector2d(0, verticalAnchor ?? 0.5), leftColor),
            Label(rightText, new Vector2d(1, verticalAnchor ?? 0.5), rightColor),
        };
    }

    public static Layouts.IColumnLayout AddValueLabel(
        this Layouts.IColumnLayout columnLayout,
        string leftText,
        IReadonlyBinding<string> rightText,
        Color? leftColor = null,
        IReadonlyBinding<Color>? rightColor = null)
    {
        return columnLayout.Add(
            ValueLabel(leftText, rightText, leftColor: leftColor, rightColor: rightColor), LineHeight);
    }
}
