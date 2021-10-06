using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct CausedDamage : IComponentEvent
    {
        public DamageResult Result { get; }

        public CausedDamage(DamageResult result)
        {
            Result = result;
        }
    }
}
