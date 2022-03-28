using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Reports;

static partial class ReportAggregator
{
    private readonly record struct GatherReports(List<IReport> reports) : IComponentEvent
    {
        private readonly List<IReport> reports = reports;

        public void AddReport(IReport report)
        {
            reports.Add(report);
        }
    }
}