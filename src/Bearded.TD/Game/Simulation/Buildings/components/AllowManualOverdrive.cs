using System;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("allowManualOverdrive")]
sealed class AllowManualOverdrive : AllowManualOverride<AllowManualOverdrive.ActiveOverdrive>, IManualOverdrive
{
    public sealed record ActiveOverdrive(Action Cancel, Overdrive Overdrive) : Override(Cancel);

    public GameObject Building => Owner;

    public bool CanBeEnabledBy(Faction faction) => CanBeOverriddenBy(faction);

    public void StartOverdrive(Action cancelOverdrive)
    {
        var control = new ActiveOverdrive(cancelOverdrive, new Overdrive());
        StartOverride(control);
    }

    public void EndOverdrive()
    {
        EndOverride();
    }

    protected override void OnOverrideStart(ActiveOverdrive control)
    {
        Owner.AddComponent(control.Overdrive);
    }

    protected override void OnOverrideEnd(ActiveOverdrive control)
    {
        Owner.RemoveComponent(control.Overdrive);
    }
}
