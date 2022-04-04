#if DEBUG

using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Debug;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

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

#endif
