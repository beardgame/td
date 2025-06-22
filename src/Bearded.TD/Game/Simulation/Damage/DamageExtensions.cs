using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

static class DamageExtensions
{
    public static void ReduceDamagePotential(this GameObject subject, UntypedDamage damagePotentialRemoved)
    {
        if (subject.TryGetSingleComponent<DamageProperty>(out var damageProperty))
            damageProperty.ReducePotential(damagePotentialRemoved);
    }

    public static UntypedDamage GetDamagePotential(this GameObject subject)
    {
        return subject.TryGetProperty<UntypedDamage>(out var damage)
            ? damage : UntypedDamage.Zero;
    }
}
