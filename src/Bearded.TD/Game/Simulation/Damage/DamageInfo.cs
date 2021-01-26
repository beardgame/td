namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct DamageInfo
    {
        public int Amount { get; }
        public DamageType Type { get; }
        public IDamageSource? Source { get; }

        public DamageInfo(int amount, DamageType type, IDamageSource? source)
        {
            Amount = amount;
            Type = type;
            Source = source;
        }

        public DamageInfo WithAdjustedAmount(int newAmount) => new(newAmount, Type, Source);
    }
}
