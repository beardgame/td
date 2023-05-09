using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Testing.Components;

sealed class EmptyTriggerSubscription : ITriggerSubscription
{
    public void Unsubscribe(ComponentEvents events) { }
}
