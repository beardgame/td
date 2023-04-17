using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Debug;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Reports;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

[ReportsOn(typeof(IDebugReport), ReportType.Debug)]
sealed class DebugReportControl : ReportControl
{
    private readonly IDebugReport report;

    private readonly Binding<string> id = new();

    public override double Height { get; }

    public DebugReportControl(IDebugReport report)
    {
        this.report = report;

        Add(new BackgroundBox(Color.Purple * 0.5f));
        var column = this.BuildFixedColumn();
        column
            .AddValueLabel("Object id", id);
        Height = column.Height;
    }

    public override void Update()
    {
        id.SetFromSource(report.Id);
    }

    public override void Dispose() {}
}
