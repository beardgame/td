using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Testing.Components;

sealed class EmptySubscription : ISubscription
{
    public void Unsubscribe(ComponentEvents events) { }
}
