using System;
using JetBrains.Annotations;

namespace Bearded.TD.Shared.Events;

[AttributeUsage(AttributeTargets.Class)]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class EventListenerAttribute(Type type) : Attribute
{
    public Type Type { get; } = type;
}
