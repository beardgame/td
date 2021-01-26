namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct DamageResult
    {
        public int DamageTaken { get; }

        public DamageResult(int damageTaken)
        {
            DamageTaken = damageTaken;
        }
    }
}
