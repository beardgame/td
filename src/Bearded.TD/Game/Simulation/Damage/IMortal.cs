namespace Bearded.TD.Game.Simulation.Damage
{
    interface IMortal
    {
        DamageResult Damage(DamageInfo damageInfo);
        void OnDeath();
    }
}
