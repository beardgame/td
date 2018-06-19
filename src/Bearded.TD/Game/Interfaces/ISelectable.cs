using Bearded.TD.Game.Meta;

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
