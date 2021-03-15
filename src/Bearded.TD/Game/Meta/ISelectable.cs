namespace Bearded.TD.Game.Meta
{
    interface ISelectable
    {
        SelectionState SelectionState { get; }

        void ResetSelection();
        void Focus();
        void Select();
    }
}
