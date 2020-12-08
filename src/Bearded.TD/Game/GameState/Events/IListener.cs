namespace Bearded.TD.Game.GameState.Events
{
    interface IListener<in TEvent>
        where TEvent : IEvent
    {
        void HandleEvent(TEvent @event);
    }
}
