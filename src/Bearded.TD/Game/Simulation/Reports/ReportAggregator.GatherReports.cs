using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Reports
{
    static partial class ReportAggregator
    {
        private readonly struct GatherReports : IComponentEvent
        {
            private readonly List<IReport> reports;

            public GatherReports(List<IReport> reports)
            {
                this.reports = reports;
            }

            public void AddReport(IReport report)
            {
                reports.Add(report);
            }
        }
    }
}
