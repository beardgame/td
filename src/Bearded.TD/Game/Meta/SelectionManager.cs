using Bearded.Utilities;

namespace Bearded.TD.Game.Meta
{
    class SelectionManager
    {
        public event GenericEventHandler<ISelectable>? ObjectSelected;
        public event GenericEventHandler<ISelectable>? ObjectDeselected;

        public ISelectable FocusedObject { get; private set; }
        public ISelectable SelectedObject { get; private set; }

        public void SelectObject(ISelectable obj)
        {
            if (obj == SelectedObject)
                return;
            ResetSelection();
            SelectedObject = obj;
            obj?.Select(this);
            ObjectSelected?.Invoke(obj);
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

            if (SelectedObject != null)
            {
                SelectedObject.ResetSelection();
                ObjectDeselected?.Invoke(SelectedObject);
            }

            SelectedObject = null;
        }

        public void ResetFocus()
        {
            FocusedObject?.ResetSelection();
            FocusedObject = null;
        }
    }
}
