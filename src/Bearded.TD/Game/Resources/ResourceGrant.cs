namespace Bearded.TD.Game.Resources
{
    readonly struct ResourceGrant
    {
        public ResourceAmount Amount { get; }
        public bool ReachedCapacity { get; }

        public ResourceGrant(ResourceAmount amount, bool reachedCapacity)
        {
            Amount = amount;
            ReachedCapacity = reachedCapacity;
        }

        public static readonly ResourceGrant Infinite = new(double.PositiveInfinity.Resources(), true);
    }
}
