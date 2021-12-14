using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("instantAim")]
class InstantAim : Component<Weapon>
{
    private IWeaponAimer? aimer;

    protected override void OnAdded()
    {
        ComponentDependencies.DependDynamic<IWeaponAimer>(Owner, Events, c => aimer = c);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Owner.IsEnabled && aimer != null)
            aimIn(aimer.AimDirection);
    }

    private void aimIn(Direction2 direction)
    {
        Owner.Turn(direction - Owner.CurrentDirection);
    }
}
