namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct DamageInfo
    {
        public int Amount { get; }
        public DamageType Type { get; }
        public IDamageOwner? Source { get; }

        public DamageInfo(int amount, DamageType type, IDamageOwner? owner)
        {
            Amount = amount;
            Type = type;
            Source = owner;
        }

        public DamageInfo WithAdjustedAmount(int newAmount) => new(newAmount, Type, Source);
    }
}
