using System;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("allowManualControl")]
sealed partial class ManualControl : AllowManualOverride<ManualControl.Override>, IManualControl
{
    public new sealed record Override(Action Cancel, CrossHair CrossHair, Overdrive Overdrive)
        : AllowManualOverride<Override>.Override(Cancel);

    public Position2 SubjectPosition => Owner.Position.XY();
    public Unit SubjectRange { get; private set; }

    public bool CanBeControlledBy(Faction faction) => CanBeOverriddenBy(faction);

    public void StartControl(IManualTarget2 target, Action cancelControl)
    {
        var control = new Override(cancelControl, new CrossHair(target), new Overdrive());
        StartOverride(control);
    }

    public void EndControl()
    {
        EndOverride();
    }

    protected override void OnOverrideStart(Override @override)
    {
        Owner.AddComponent(@override.Overdrive);
        Owner.AddComponent(@override.CrossHair);

        SubjectRange = 3.U();

        foreach (var turret in Owner.GetComponents<ITurret>())
        {
            turret.OverrideTargeting(@override.CrossHair);
            if (turret.Weapon.TryGetSingleComponent<IWeaponRange>(out var range))
                SubjectRange = Math.Max(SubjectRange.NumericValue, range.Range.NumericValue).U();
        }
    }

    protected override void OnOverrideEnd(Override @override)
    {
        Owner.RemoveComponent(@override.Overdrive);
        Owner.RemoveComponent(@override.CrossHair);

        foreach (var turret in Owner.GetComponents<ITurret>())
        {
            turret.StopTargetOverride();
        }
    }
}

interface IManualControl
{
    Position2 SubjectPosition { get; }
    Unit SubjectRange { get; }

    bool CanBeControlledBy(Faction faction);
    void StartControl(IManualTarget2 target, Action cancelControl);
    void EndControl();
}
