using Bearded.TD.Game.UI;

namespace Bearded.TD.Game
{
    interface ISelectable
    {
        void ResetSelection();
        void Focus(SelectionManager selectionManager);
        void Select(SelectionManager selectionManager);
    }
}
