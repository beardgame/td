using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Damage;

namespace Bearded.TD.Game.Simulation.Components.Damage
{
    sealed class DamageExecutor
    {
        private readonly ComponentEvents events;

        public DamageExecutor(ComponentEvents events)
        {
            this.events = events;
        }

        public void Damage(DamageInfo damageInfo)
        {
            var takeDamage = new TakeDamage(damageInfo);
            events.Preview(ref takeDamage);
        }
    }
}
