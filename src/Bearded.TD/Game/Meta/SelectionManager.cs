using Bearded.Utilities;

namespace Bearded.TD.Game.Meta;

sealed class SelectionManager
{
    public delegate void UndoDelegate();

    public event GenericEventHandler<ISelectable>? ObjectFocused;
    public event GenericEventHandler<ISelectable>? ObjectUnfocused;
    public event GenericEventHandler<ISelectable>? ObjectSelected;
    public event GenericEventHandler<ISelectable>? ObjectDeselected;

    private ISelectable? focusedObject;
    private ISelectable? selectedObject;

    public void SelectObject(ISelectable obj)
    {
        if (obj == selectedObject)
        {
            return;
        }

        ResetSelection();
        selectedObject = obj;
        obj.Select(ResetSelection);
        ObjectSelected?.Invoke(obj);
    }

    public void FocusObject(ISelectable obj)
    {
        if (obj == selectedObject || obj == focusedObject)
        {
            return;
        }

        ResetFocus();
        focusedObject = obj;
        obj.Focus(ResetFocus);
        ObjectFocused?.Invoke(obj);
    }

    public void ResetSelection()
    {
        ResetFocus();

        if (selectedObject != null)
        {
            selectedObject.ResetSelection();
            ObjectDeselected?.Invoke(selectedObject);
        }

        selectedObject = null;
    }

    public void ResetFocus()
    {
        if (focusedObject != null)
        {
            focusedObject.ResetSelection();
            ObjectUnfocused?.Invoke(focusedObject);
        }

        focusedObject = null;
    }
}
