using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Weapons;

static class WeaponFirer
{
    public static bool FireWeapon(ComponentEvents events, UntypedDamage damage)
    {
        var preview = new PreviewFireWeapon(damage, false);
        events.Preview(ref preview);
        if (preview.IsCancelled)
        {
            events.Send(new WeaponMisfired());
            return false;
        }

        events.Send(new FireWeapon(damage));
        return true;
    }
}
