namespace Bearded.TD.Game.GameState.Damage
{
    struct DamageInfo
    {
        public int Amount { get; }
        public DamageType Type { get; }
        public IDamageOwner Source { get; }

        public DamageInfo(int amount, DamageType type, IDamageOwner owner)
        {
            Amount = amount;
            Type = type;
            Source = owner;
        }
    }
}
