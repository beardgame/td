using Bearded.TD.Game.Simulation.Components.Events;

namespace Bearded.TD.Game.Simulation.Damage
{
    sealed class DamageExecutor
    {
        private readonly ComponentEvents events;

        public DamageExecutor(ComponentEvents events)
        {
            this.events = events;
        }

        public DamageResult Damage(DamageInfo damageInfo)
        {
            var takeDamage = new TakeDamage(damageInfo);
            events.Preview(ref takeDamage);
            return new DamageResult(takeDamage.DamageTaken);
        }
    }
}
