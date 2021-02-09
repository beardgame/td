namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct DamageInfo
    {
        public HitPoints Amount { get; }
        public DamageType Type { get; }
        public IDamageSource? Source { get; }

        public DamageInfo(HitPoints amount, DamageType type, IDamageSource? source)
        {
            Amount = amount;
            Type = type;
            Source = source;
        }

        public DamageInfo WithAdjustedAmount(HitPoints newAmount) => new(newAmount, Type, Source);
    }
}
