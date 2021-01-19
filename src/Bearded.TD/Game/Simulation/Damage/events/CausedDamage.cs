using Bearded.TD.Game.Simulation.Components.Events;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct CausedDamage : IComponentEvent
    {
        public IMortal Target { get; }
        public DamageResult Result { get; }

        public CausedDamage(IMortal target, DamageResult result)
        {
            Target = target;
            Result = result;
        }
    }
}
