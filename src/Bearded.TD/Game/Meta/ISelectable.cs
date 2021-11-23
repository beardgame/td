using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Meta
{
    interface ISelectable
    {
        bool IsSelectable { get; }
        IReportSubject Subject { get; }

        void ResetSelection();
        void Focus(SelectionManager.UndoDelegate undoFocus);
        void Select(SelectionManager.UndoDelegate undoSelection);
    }
}
