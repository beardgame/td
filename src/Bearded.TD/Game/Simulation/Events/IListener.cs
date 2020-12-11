namespace Bearded.TD.Game.Simulation.Events
{
    interface IListener<in TEvent>
        where TEvent : IEvent
    {
        void HandleEvent(TEvent @event);
    }
}
