using Bearded.TD.Game.Simulation.Damage;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Components.Events
{
    readonly struct TakeDamage : IComponentPreviewEvent
    {
        public DamageInfo Damage { get; }
        public int DamageTaken { get; }

        public TakeDamage(DamageInfo damage, int damageTaken = 0)
        {
            Damage = damage;
            DamageTaken = damageTaken;
        }

        public TakeDamage DamageAdded(int damageAmount)
        {
            Argument.Satisfies(DamageTaken + damageAmount <= Damage.Amount);
            return new TakeDamage(Damage, DamageTaken + damageAmount);
        }
    }
}
