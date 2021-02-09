using Bearded.TD.Game.Simulation.Components.Events;

namespace Bearded.TD.Game.Simulation.Reports
{
    readonly struct ReportAdded : IComponentEvent
    {
        public IReport Report { get; }

        public ReportAdded(IReport report)
        {
            Report = report;
        }
    }
}
