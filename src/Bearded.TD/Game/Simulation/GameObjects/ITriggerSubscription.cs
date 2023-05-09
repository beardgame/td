namespace Bearded.TD.Game.Simulation.GameObjects;

interface ITriggerSubscription
{
    void Unsubscribe(ComponentEvents events);
}
