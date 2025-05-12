namespace Bearded.TD.Game.Simulation.Damage;

// Note: all implementations should be added to DamageModifiers#Order
interface IDamageModifier
{
    void ModifyDamage(ref DamagePreview preview);
}
