using System;

namespace Bearded.TD.Game.Simulation.Reports;

[Obsolete]
interface IReport
{
    ReportType Type { get; }
}
