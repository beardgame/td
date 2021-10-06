using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct AttributedDamage : IComponentEvent
    {
        public DamageResult Result { get; }

        public AttributedDamage(DamageResult result)
        {
            Result = result;
        }
    }
}
