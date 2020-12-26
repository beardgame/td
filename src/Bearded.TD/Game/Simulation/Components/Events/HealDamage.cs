namespace Bearded.TD.Game.Simulation.Components.Events
{
    readonly struct HealDamage : IComponentEvent
    {
        public int Amount { get; }

        public HealDamage(int amount)
        {
            Amount = amount;
        }
    }
}
