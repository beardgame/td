using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("allowManualControl")]
sealed partial class AllowManualControl : AllowManualOverride<AllowManualControl.Control>, IManualControlReport
{
    public new sealed record Control(Action Cancel, CrossHair CrossHair, Overdrive Overdrive)
        : AllowManualOverride<Control>.Control(Cancel);

    protected override IReport Report => this;
    public ReportType Type => ReportType.ManualControl;

    public Position2 SubjectPosition => Owner.Position.XY();
    public Unit SubjectRange { get; private set; }

    public void StartControl(IManualTarget2 target, Action cancelControl)
    {
        var control = new Control(cancelControl, new CrossHair(target), new Overdrive());
        StartControl(control);
    }

    protected override void OnOverrideStart(Control control)
    {
        Owner.AddComponent(control.Overdrive);
        Owner.AddComponent(control.CrossHair);

        SubjectRange = 3.U();

        foreach (var turret in Owner.GetComponents<ITurret>())
        {
            turret.OverrideTargeting(control.CrossHair);
            if (turret.Weapon.TryGetSingleComponent<IWeaponRange>(out var range))
                SubjectRange = Math.Max(SubjectRange.NumericValue, range.Range.NumericValue).U();
        }
    }

    protected override void OnOverrideEnd(Control control)
    {
        Owner.RemoveComponent(control.Overdrive);
        Owner.RemoveComponent(control.CrossHair);

        foreach (var turret in Owner.GetComponents<ITurret>())
        {
            turret.StopTargetOverride();
        }
    }
}
