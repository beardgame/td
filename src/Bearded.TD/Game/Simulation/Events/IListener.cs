namespace Bearded.TD.Game.Simulation.Events
{
    interface IListener<in TEvent>
        where TEvent : IEvent
    {
        void HandleEvent(TEvent @event);
    }

    interface IPreviewListener<TEvent>
        where TEvent : IPreviewEvent
    {
        void PreviewEvent(ref TEvent @event);
    }
}
