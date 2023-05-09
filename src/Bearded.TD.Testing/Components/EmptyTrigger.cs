using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Testing.Components;

sealed class EmptyTrigger : ITrigger
{
    public ISubscription Subscribe(ComponentEvents events, Action action) => new EmptySubscription();
}
