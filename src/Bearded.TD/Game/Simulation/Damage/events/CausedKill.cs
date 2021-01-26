using Bearded.TD.Game.Simulation.Components.Events;

namespace Bearded.TD.Game.Simulation.Damage
{
    readonly struct CausedKill : IComponentEvent
    {
        public IMortal Target { get; }

        public CausedKill(IMortal target)
        {
            Target = target;
        }
    }
}
