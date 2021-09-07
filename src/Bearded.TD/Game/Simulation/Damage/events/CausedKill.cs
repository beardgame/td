using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct CausedKill : IComponentEvent
    {
        public IDamageTarget Target { get; }

        public CausedKill(IDamageTarget target)
        {
            Target = target;
        }
    }
}
