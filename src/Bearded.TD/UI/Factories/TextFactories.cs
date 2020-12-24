using amulware.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.UI.Text;

namespace Bearded.TD.UI.Factories
{
    static class TextFactories
    {
        public static Label Header(string text, Color? color = null) => new(text)
        {
            Color = color ?? TextColor,
            FontSize = HeaderFontSize,
            TextAnchor = Controls.Label.TextAnchorLeft
        };

        public static Layouts.IColumnLayout AddHeader(
            this Layouts.IColumnLayout columnLayout, string text, Color? color = null)
        {
            return columnLayout.Add(Header(text, color), HeaderLineHeight);
        }

        public static Label Label(string text, Vector2d? textAnchor = null, Color? color = null) => new(text)
        {
            Color = color ?? TextColor,
            FontSize = FontSize,
            TextAnchor = textAnchor ?? Controls.Label.TextAnchorCenter
        };

        public static Layouts.IColumnLayout AddLabel(
            this Layouts.IColumnLayout columnLayout, string text, Color? color = null)
        {
            return columnLayout.Add(Label(text, color: color), LineHeight);
        }

        public static Control ValueLabel(
            string leftText,
            Binding<string> rightText,
            double? verticalAnchor = null,
            Color? leftColor = null,
            Color? rightColor = null)
        {
            var valueLabel = Label(rightText.Value, new Vector2d(1, verticalAnchor ?? 0.5), rightColor);
            rightText.SourceUpdated += newValue => valueLabel.Text = newValue;

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
            Color? rightColor = null)
        {
            return columnLayout.Add(
                ValueLabel(leftText, rightText, leftColor: leftColor, rightColor: rightColor), LineHeight);
        }
    }
}
