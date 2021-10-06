using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct AttributedKill : IComponentEvent
    {
        public IDamageTarget Target { get; }

        public AttributedKill(IDamageTarget target)
        {
            Target = target;
        }
    }
}
