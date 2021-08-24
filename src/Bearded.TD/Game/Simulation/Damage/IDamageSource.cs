namespace Bearded.TD.Game.Simulation.Damage
{
    interface IDamageSource
    {
        void AttributeDamage(IDamageTarget target, DamageResult result);
        void AttributeKill(IDamageTarget target);
    }
}
