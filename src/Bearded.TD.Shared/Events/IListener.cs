namespace Bearded.TD.Shared.Events;

public interface IListener<in TEvent>
    where TEvent : IEvent
{
    void HandleEvent(TEvent @event);
}

public interface IPreviewListener<TEvent>
    where TEvent : IPreviewEvent
{
    void PreviewEvent(ref TEvent @event);
}
