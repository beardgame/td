using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct TakeDamage : IComponentEvent
    {
        public DamageResult Damage { get; }

        public TakeDamage(DamageResult damage)
        {
            Damage = damage;
        }
    }
}
