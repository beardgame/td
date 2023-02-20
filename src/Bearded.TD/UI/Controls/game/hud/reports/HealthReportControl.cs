using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

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
