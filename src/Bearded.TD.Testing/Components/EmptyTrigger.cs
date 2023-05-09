using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Testing.Components;

sealed class EmptyTrigger : ITrigger
{
    public ITriggerSubscription Subscribe(ComponentEvents events, Action action) => new EmptyTriggerSubscription();
}
