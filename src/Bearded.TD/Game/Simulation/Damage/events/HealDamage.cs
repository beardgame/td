using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct HealDamage : IComponentEvent
    {
        public HitPoints Amount { get; }

        public HealDamage(HitPoints amount)
        {
            Amount = amount;
        }
    }
}
