namespace Bearded.TD.Game.Simulation.Damage
{
    interface IMortal
    {
        void Damage(DamageInfo damageInfo);
        void OnDeath();
    }
}
