using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Reports
{
    static partial class ReportAggregator
    {
        private readonly record struct ReportAdded(IReport Report) : IComponentEvent;
    }
}
