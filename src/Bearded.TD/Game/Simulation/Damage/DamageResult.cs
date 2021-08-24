namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct DamageResult
    {
        public DamageInfo Damage { get; }

        public DamageResult(DamageInfo damage)
        {
            Damage = damage;
        }
    }
}
