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
    }
}