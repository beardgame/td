using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Reports;

static partial class ReportAggregator
{
    private readonly record struct ReportRemoved(IReport Report) : IComponentEvent;
}