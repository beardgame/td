using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Reports;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

[ReportsOn(typeof(ITargetingReport), ReportType.EntityMode)]
sealed class TargetingReportControl : ReportControl
{
    private readonly ITargetingReport report;
    private readonly Binding<ITargetingMode> targetingMode = new();

    public override double Height { get; }

    public TargetingReportControl(GameInstance game, ITargetingReport report)
    {
        this.report = report;
        targetingMode.SetFromSource(report.TargetingMode);

        var column = this.BuildFixedColumn();
        column.AddForm(form =>
            form.AddDropdownSelectRow("Targeting", report.AvailableTargetingModes, mode => mode.Name, targetingMode));
        Height = column.Height;

        targetingMode.ControlUpdated += newMode => game.Request(SetTargetingMode.Request, report.Object, newMode);
    }

    public override void Update()
    {
        if (report.TargetingMode != targetingMode.Value)
        {
            targetingMode.SetFromSource(report.TargetingMode);
        }
    }

    public override void Dispose() { }
}
