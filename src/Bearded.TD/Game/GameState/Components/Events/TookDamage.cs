using Bearded.TD.Game.GameState.Damage;

namespace Bearded.TD.Game.GameState.Components.Events
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
