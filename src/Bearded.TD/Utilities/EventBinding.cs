using Bearded.Utilities;

namespace Bearded.TD.Utilities;

public sealed class EventBinding : IReadonlyBinding<Void>
{
    public Void Value => default;

    public event GenericEventHandler<Void>? ControlUpdated;
    public event GenericEventHandler<Void>? SourceUpdated;

    public void InvokeFromControl()
    {
        ControlUpdated?.Invoke(default);
    }

    public void InvokeFromSource()
    {
        SourceUpdated?.Invoke(default);
    }
}
