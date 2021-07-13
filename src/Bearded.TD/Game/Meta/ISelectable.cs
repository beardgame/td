using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Meta
{
    interface ISelectable
    {
        IReportSubject Subject { get; }

        void ResetSelection();
        void Focus();
        void Select();
    }
}
