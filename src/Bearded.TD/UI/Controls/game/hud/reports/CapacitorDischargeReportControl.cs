using System;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class CapacitorDischargeReportControl : ReportControl
{
    private readonly IDischargeModeReport report;
    private readonly Binding<CapacitorDischargeMode> dischargeMode = new();

    public override double Height { get; }

    public CapacitorDischargeReportControl(GameInstance game, IDischargeModeReport report)
    {
        this.report = report;
        dischargeMode.SetFromSource(report.DischargeMode);

        var column = this.BuildFixedColumn();
        column.AddForm(form =>
            form.AddDropdownSelectRow(
                "Capacitor", Enum.GetValues<CapacitorDischargeMode>(), mode => mode.Name(), dischargeMode));
        Height = column.Height;

        dischargeMode.ControlUpdated += newMode => game.Request(SetDischargeMode.Request, report.Object, newMode);
    }

    public override void Update()
    {
        if (report.DischargeMode != dischargeMode.Value)
        {
            dischargeMode.SetFromSource(report.DischargeMode);
        }
    }

    public override void Dispose() { }
}
