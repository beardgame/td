using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("instantAim")]
class InstantAim : Component<ComponentGameObject>
{
    private IWeaponState weapon = null!;
    private IWeaponAimer? aimer;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponState>(Owner, Events, c => weapon = c);
        ComponentDependencies.DependDynamic<IWeaponAimer>(Owner, Events, c => aimer = c);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (weapon.IsEnabled && aimer != null)
            aimIn(aimer.AimDirection);
    }

    private void aimIn(Direction2 direction)
    {
        weapon.Turn(direction - weapon.Direction);
    }
}
