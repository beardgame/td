using System;
using Bearded.TD.UI.Controls;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories
{
    [Obsolete]
    static class LegacyDefault
    {
        [Obsolete]
        public static Button Button(string title, double fontSize = Constants.UI.Button.FontSize)
        {
            return new Button().WithDefaultStyle(title, fontSize);
        }

        [Obsolete]
        public static Button WithDefaultStyle(this Button button, string title, double fontSize = Constants.UI.Button.FontSize)
        {
            return button.WithDefaultStyle(new Label
            {
                Text = title,
                FontSize = fontSize,
            });
        }

        [Obsolete]
        public static Button WithDefaultStyle(this Button button, Control labelControl)
        {
            button.Add(labelControl);
            button.Add(new Border());
            button.Add(new ButtonBackgroundEffect());
            return button;
        }
    }
}
