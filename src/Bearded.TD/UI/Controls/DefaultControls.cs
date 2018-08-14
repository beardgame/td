using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    static class DefaultControls
    {
        public static Button Button(string title)
        {
            return new Button
            {
                new Label(title),
                new ButtonBackgroundEffect()
            };
        }
    }
}
