using Bearded.TD.Game.Damage;

namespace Bearded.TD.Game.Components.Events
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
