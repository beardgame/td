using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.UI.Factories
{
    sealed record ButtonAction
    {
        public string Label { get; }
        public VoidEventHandler OnClick { get; }
        public Binding<bool>? IsEnabled { get; }

        public ButtonAction(string label, VoidEventHandler onClick, Binding<bool>? isEnabled)
        {
            Label = label;
            OnClick = onClick;
            IsEnabled = isEnabled;
        }
    }
}
