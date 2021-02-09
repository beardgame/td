namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct DamageResult
    {
        public HitPoints DamageTaken { get; }

        public DamageResult(HitPoints damageTaken)
        {
            DamageTaken = damageTaken;
        }
    }
}
