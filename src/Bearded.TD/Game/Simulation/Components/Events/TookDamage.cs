using Bearded.TD.Game.Simulation.Damage;

namespace Bearded.TD.Game.Simulation.Components.Events
{
    struct TookDamage : IComponentEvent
    {
        public DamageInfo Damage { get; }

        public TookDamage(DamageInfo damage)
        {
            Damage = damage;
        }
    }
}
