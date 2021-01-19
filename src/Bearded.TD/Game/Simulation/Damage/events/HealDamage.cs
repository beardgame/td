using Bearded.TD.Game.Simulation.Components.Events;

namespace Bearded.TD.Game.Simulation.Damage
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
