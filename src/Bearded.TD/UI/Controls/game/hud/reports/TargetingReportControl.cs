using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class TargetingReportControl : ReportControl
{
    private readonly Binding<ITargetingMode> targetingMode = new();

    public override double Height { get; }

    public TargetingReportControl(ITargetingReport report)
    {
        targetingMode.SetFromSource(report.TargetingMode);

        var column = this.BuildFixedColumn();
        column.AddForm(form =>
            form.AddDropdownSelectRow("Targeting", report.AvailableTargetingModes, mode => mode.Name, targetingMode));
        Height = column.Height;

        targetingMode.ControlUpdated += newMode => report.TargetingMode = newMode;
    }

    public override void Update() { }

    public override void Dispose() { }
}
