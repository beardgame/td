using Bearded.TD.UI.Controls;
using OpenTK;
using static Bearded.TD.Constants.UI.Text;

namespace Bearded.TD.UI.Factories
{
    static class TextFactories
    {
        public static Label Header(string text) => new Label(text)
        {
            FontSize = HeaderFontSize,
            TextAnchor = Controls.Label.TextAnchorLeft
        };

        public static LayoutFactories.IColumnBuilder AddHeader(
            this LayoutFactories.IColumnBuilder columnBuilder, string text) =>
            columnBuilder.Add(Header(text), HeaderLineHeight);

        public static Label Label(string text, Vector2d? textAnchor = null) => new Label(text)
        {
            FontSize = FontSize,
            TextAnchor = textAnchor ?? Controls.Label.TextAnchorCenter
        };
    }
}
