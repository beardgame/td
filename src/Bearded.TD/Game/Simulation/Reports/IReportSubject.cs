using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Reports;

interface IReportSubject
{
    public string Name { get; }
    public Faction? Faction { get; }
    public IReadOnlyCollection<IReport> Reports { get; }
    event VoidEventHandler? ReportsUpdated;
}