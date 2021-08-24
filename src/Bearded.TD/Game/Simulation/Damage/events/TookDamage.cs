using Bearded.TD.Game.Simulation.Components.Events;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct TookDamage : IComponentEvent
    {
        public IDamageSource? Source { get; }
        public DamageResult Damage { get; }

        public TookDamage(IDamageSource? source, DamageResult damage)
        {
            Source = source;
            Damage = damage;
        }
    }
}
