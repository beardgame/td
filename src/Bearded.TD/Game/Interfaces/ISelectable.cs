using Bearded.TD.UI.Model;

namespace Bearded.TD.Game
{
    interface ISelectable
    {
        SelectionState SelectionState { get; }

        void ResetSelection();
        void Focus(SelectionManager selectionManager);
        void Select(SelectionManager selectionManager);
    }
}
