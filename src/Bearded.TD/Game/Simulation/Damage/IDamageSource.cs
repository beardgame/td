namespace Bearded.TD.Game.Simulation.Damage
{
    interface IDamageSource
    {
        void AttributeDamage(IMortal target, DamageResult result);
        void AttributeKill(IMortal target);
    }
}
