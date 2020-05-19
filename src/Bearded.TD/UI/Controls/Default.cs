using System;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    static class Default
    {
        public static Button Button(string title, double fontSize = 24)
        {
            return new Button().WithDefaultStyle(title, fontSize);
        }

        public static Button Button(Func<string> titleProvider, double fontSize = 24)
        {
            return new Button().WithDefaultStyle(titleProvider, fontSize);
        }

        public static Button WithDefaultStyle(this Button button, string title, double fontSize = 24)
        {
            return button.WithDefaultStyle(new Label
            {
                Text = title,
                FontSize = fontSize,
            });
        }

        public static Button WithDefaultStyle(this Button button, Func<string> titleProvider, double fontSize = 24)
        {
            return button.WithDefaultStyle(new DynamicLabel(titleProvider)
            {
                FontSize = fontSize,
            });
        }

        public static Button WithDefaultStyle(this Button button, Control labelControl)
        {
            button.Add(labelControl);
            button.Add(new Border());
            button.Add(new ButtonBackgroundEffect());
            return button;
        }
    }
}
