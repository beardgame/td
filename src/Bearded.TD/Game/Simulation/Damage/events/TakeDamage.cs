using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct TakeDamage : IComponentEvent
    {
        public DamageResult Damage { get; }
        public IDamageSource? Source { get; }

        public TakeDamage(DamageResult damage, IDamageSource? source)
        {
            Damage = damage;
            Source = source;
        }
    }
}
