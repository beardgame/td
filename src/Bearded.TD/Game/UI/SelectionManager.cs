namespace Bearded.TD.Game.UI
{
    class SelectionManager
    {
        public ISelectable FocusedObject { get; private set; }
        public ISelectable SelectedObject { get; private set; }

        public void SelectObject(ISelectable obj)
        {
            if (obj == SelectedObject)
                return;
            ResetSelection();
            SelectedObject = obj;
            obj?.Select(this);
        }

        public void FocusObject(ISelectable obj)
        {
            if (obj == SelectedObject || obj == FocusedObject)
                return;
            ResetFocus();
            FocusedObject = obj;
            obj?.Focus(this);
        }

        public void ResetSelection()
        {
            ResetFocus();
            SelectedObject?.ResetSelection();
            SelectedObject = null;
        }

        public void ResetFocus()
        {
            FocusedObject?.ResetSelection();
            FocusedObject = null;
        }
    }
}
