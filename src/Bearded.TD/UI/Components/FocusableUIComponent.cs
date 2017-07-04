using Bearded.Utilities;

namespace Bearded.TD.UI.Components
{
    abstract class FocusableUIComponent : UIComponent, IFocusable
    {
        protected bool IsFocused { get; private set; }

        public event GenericEventHandler<IFocusable> Focused;
        public event GenericEventHandler<IFocusable> Unfocused;

        protected FocusableUIComponent(Bounds bounds) : base(bounds) { }

        public void Focus()
        {
            if (IsFocused) return;
            IsFocused = true;
            Focused?.Invoke(this);
        }

        public void Unfocus()
        {
            if (!IsFocused) return;
            IsFocused = false;
            Unfocused?.Invoke(this);
        }

        protected void SetFocus(bool value)
        {
            if (value)
                Focus();
            else
                Unfocus();
        }
    }
}
