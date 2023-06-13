using System;
using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.UI.Reports;

sealed class ReportsOnAttribute : Attribute
{
    public Type Type { get; }
    public ReportType ReportType { get; }

    public ReportsOnAttribute(Type type, ReportType reportType)
    {
        Type = type;
        ReportType = reportType;
    }
}
