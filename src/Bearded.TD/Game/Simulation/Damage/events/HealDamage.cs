using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct HealDamage : IComponentEvent
    {
        public HealResult Heal { get; }

        public HealDamage(HealResult heal)
        {
            Heal = heal;
        }
    }
}
