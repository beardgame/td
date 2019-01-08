using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    static class Default
    {
        public static Button Button(string title, double fontSize = 24)
        {
            return new Button().WithDefaultStyle(title, fontSize);
        }

        public static Button WithDefaultStyle(this Button button, string title, double fontSize = 24)
        {
            button.Add(new Label
            {
                Text = title,
                FontSize = fontSize
            });
            button.Add(new ButtonBackgroundEffect());
            return button;
        }
    }
}
