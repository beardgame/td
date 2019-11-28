using Bearded.TD.Game.Damage;

namespace Bearded.TD.Game.Components.Events
{
    struct TakeDamage : IEvent
    {
        public DamageInfo Damage { get; }

        public TakeDamage(DamageInfo damage)
        {
            Damage = damage;
        }
    }
}
