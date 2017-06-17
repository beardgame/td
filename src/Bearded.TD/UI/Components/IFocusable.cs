using Bearded.Utilities;

namespace Bearded.TD.UI.Components
{
    interface IFocusable
    {
        void Focus();
        void Unfocus();

        event GenericEventHandler<IFocusable> Focused;
        event GenericEventHandler<IFocusable> Unfocused;
    }
}
