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
    public event VoidEventHandler? ReportsUpdated;
}