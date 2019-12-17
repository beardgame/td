namespace Bearded.TD.Game.Events
{
    interface IListener<in TEvent>
        where TEvent : IEvent
    {
        void HandleEvent(TEvent @event);
    }
}
