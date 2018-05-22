namespace Bearded.TD.Game.Resources
{
    struct ResourceGrant
    {
        public double Amount { get; }
        public bool ReachedCapacity { get; }

        public ResourceGrant(double amount, bool reachedCapacity)
        {
            Amount = amount;
            ReachedCapacity = reachedCapacity;
        }

        public static readonly ResourceGrant Infinite = new ResourceGrant(double.PositiveInfinity, true);
    }
}
