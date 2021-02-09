using Bearded.TD.Game.Simulation.Components.Events;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct TakeDamage : IComponentPreviewEvent
    {
        public DamageInfo Damage { get; }
        public HitPoints DamageTaken { get; }

        public TakeDamage(DamageInfo damage) : this(damage, HitPoints.Zero) {}

        public TakeDamage(DamageInfo damage, HitPoints damageTaken)
        {
            Damage = damage;
            DamageTaken = damageTaken;
        }

        public TakeDamage DamageAdded(HitPoints damageAmount)
        {
            Argument.Satisfies(DamageTaken + damageAmount <= Damage.Amount);
            return new TakeDamage(Damage, DamageTaken + damageAmount);
        }
    }
}
