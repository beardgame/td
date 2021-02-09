using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories
{
    interface IReportControlFactory
    {
        Control CreateForReport(IReport report, Disposer disposer, out double height);
    }
}
