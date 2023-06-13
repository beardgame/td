using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Reports;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

[ReportsOn(typeof(IHealthReport), ReportType.EntityProperties)]
sealed class HealthReportControl : ReportControl
{
    public override double Height { get; }

    private readonly Binding<string> health = new();

    private readonly IHealthReport report;

    public HealthReportControl(IHealthReport report)
    {
        this.report = report;

        var column = this.BuildFixedColumn();
        column
            .AddValueLabel("Health", health);
        Height = column.Height;

        Update();
    }

    public override void Update()
    {
        health.SetFromSource($"{report.CurrentHealth.ToUiString()} / {report.MaxHealth.ToUiString()}");
    }

    public override void Dispose() {}
}
