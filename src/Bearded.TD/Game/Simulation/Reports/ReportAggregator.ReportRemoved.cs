using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Reports
{
    static partial class ReportAggregator
    {
        private readonly struct ReportRemoved : IComponentEvent
        {
            public IReport Report { get; }

            public ReportRemoved(IReport report)
            {
                Report = report;
            }
        }
    }
}
