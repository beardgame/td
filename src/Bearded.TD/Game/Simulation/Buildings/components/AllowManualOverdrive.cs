using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("allowManualOverdrive")]
sealed class AllowManualOverdrive : AllowManualOverride<AllowManualOverdrive.Control>, IManualOverdriveReport
{
    public new sealed record Control(Action Cancel, Overdrive Overdrive)
        : AllowManualOverride<Control>.Control(Cancel);

    protected override IReport Report => this;
    public ReportType Type => ReportType.ManualControl;

    public void StartControl(Action cancelOverdrive)
    {
        var control = new Control(cancelOverdrive, new Overdrive());
        StartControl(control);
    }

    protected override void OnOverrideStart(Control control)
    {
        Owner.AddComponent(control.Overdrive);
    }

    protected override void OnOverrideEnd(Control control)
    {
        Owner.RemoveComponent(control.Overdrive);
    }
}
