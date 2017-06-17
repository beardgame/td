namespace Bearded.TD.UI.Components
{
    class FocusContainer
    {
        public IFocusable FocusedObject { get; private set; }

        public void RegisterFocusable(IFocusable focusable)
        {
            focusable.Focused += onObjectFocused;
            focusable.Unfocused += onObjectUnfocused;
        }

        private void onObjectFocused(IFocusable focusable)
        {
            FocusedObject?.Unfocus();
            FocusedObject = focusable;
        }

        private void onObjectUnfocused(IFocusable focusable)
        {
            if (focusable == FocusedObject)
                FocusedObject = null;
        }
    }
}
