using Bearded.TD.Game.Simulation.Damage;

namespace Bearded.TD.Game.Simulation.Components.Events
{
    struct TakeDamage : IComponentEvent
    {
        public DamageInfo Damage { get; }

        public TakeDamage(DamageInfo damage)
        {
            Damage = damage;
        }
    }
}
