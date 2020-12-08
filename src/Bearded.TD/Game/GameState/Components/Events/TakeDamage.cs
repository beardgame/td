using Bearded.TD.Game.GameState.Damage;

namespace Bearded.TD.Game.GameState.Components.Events
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
