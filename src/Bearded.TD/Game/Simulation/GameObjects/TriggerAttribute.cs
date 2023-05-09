using System;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.GameObjects;

[AttributeUsage(AttributeTargets.Struct)]
[BaseTypeRequired(typeof(IComponentEvent))]
public sealed class TriggerAttribute : Attribute
{
    public string Id { get; }

    public TriggerAttribute(string id)
    {
        Id = id;
    }
}
