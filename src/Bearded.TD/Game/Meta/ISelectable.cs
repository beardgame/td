namespace Bearded.TD.Game.Meta
{
    interface ISelectable
    {
        SelectionState SelectionState { get; }

        void ResetSelection();
        void Focus(SelectionManager selectionManager);
        void Select(SelectionManager selectionManager);
    }
}
