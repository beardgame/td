using Bearded.TD.UI.Controls;
using OpenTK;
using static Bearded.TD.UI.Controls.Label;

namespace Bearded.TD.UI.Factories
{
    static class LabelFactories
    {
        public static Label Label(string text, Vector2d? textAnchor = null) => new Label(text)
        {
            FontSize = Constants.UI.FontSize,
            TextAnchor = textAnchor ?? TextAnchorCenter
        };
    }
}
