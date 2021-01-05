using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories
{
    static class ButtonExtensions
    {
        public static Button BindIsEnabled(this Button button, Binding<bool>? isEnabled)
        {
            if (isEnabled == null)
            {
                return button;
            }

            button.IsEnabled = isEnabled.Value;
            isEnabled.SourceUpdated += enabled => button.IsEnabled = enabled;

            return button;
        }
    }
}
