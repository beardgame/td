using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Reports;

sealed class EmptyReportSubject : IReportSubject
{
    public string Name => "";
    public Faction? Faction => null;

    public IReadOnlyCollection<IReport> Reports => ImmutableArray<IReport>.Empty;
#pragma warning disable CS0067
    public event VoidEventHandler? ReportsUpdated;
#pragma warning restore CS0067
}
