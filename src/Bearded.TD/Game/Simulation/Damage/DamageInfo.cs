namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct DamageInfo
    {
        public HitPoints Amount { get; }
        public DamageType Type { get; }

        public DamageInfo(HitPoints amount, DamageType type)
        {
            Amount = amount;
            Type = type;
        }

        public DamageInfo WithAdjustedAmount(HitPoints newAmount) => new(newAmount, Type);
    }
}
