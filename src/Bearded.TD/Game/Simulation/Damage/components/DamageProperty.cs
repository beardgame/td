using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class DamageProperty(UntypedDamage damagePotential) : Component, IProperty<UntypedDamage>
{
    public UntypedDamage Value { get; private set; } = damagePotential;

    public void ReducePotential(UntypedDamage potentialRemoved)
    {
        Value -= UntypedDamage.Min(potentialRemoved, Value);
    }
}
