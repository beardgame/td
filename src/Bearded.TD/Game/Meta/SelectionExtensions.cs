using System;

namespace Bearded.TD.Game.Meta
{
    static class SelectionExtensions
    {
        public static void CheckCurrentlyFocused(this SelectionManager selectionManager, ISelectable selectable)
        {
            if (selectionManager.FocusedObject != selectable)
                throw new Exception("Cannot focus an object that is not the currently focused object.");
        }

        public static void CheckCurrentlySelected(this SelectionManager selectionManager, ISelectable selectable)
        {
            if (selectionManager.SelectedObject != selectable)
                throw new Exception("Cannot select an object that is not the currently focused object.");
        }
    }
}
