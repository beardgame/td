using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    static class Default
    {
        public static Button Button(string title, double fontSize = 24)
        {
            return new Button
            {
                new Label
                {
                    Text = title,
                    FontSize = fontSize
                },
                new ButtonBackgroundEffect()
            };
        }
    }
}
