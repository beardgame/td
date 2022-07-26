using System;
using JetBrains.Annotations;

namespace Bearded.TD.Shared.Events;

[AttributeUsage(AttributeTargets.Class)]
[UsedImplicitly]
public sealed class EventListenerAttribute : Attribute
{
    public Type Type { get; }

    public EventListenerAttribute(Type type)
    {
        Type = type;
    }
}
