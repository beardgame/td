using Bearded.TD.Game.Simulation.GameObjects;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Reports;

static partial class ReportAggregator
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)] // bug: Report is accessed, but somehow marked a never accessed
    private readonly record struct ReportAdded(IReport Report) : IComponentEvent;
}
