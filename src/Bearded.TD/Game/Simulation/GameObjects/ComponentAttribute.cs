using System;
using Bearded.TD.Content.Behaviors;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.GameObjects;

[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(IComponent))]
[MeansImplicitUse]
sealed class ComponentAttribute : Attribute, IBehaviorAttribute
{
    public string Id { get; }

    public ComponentAttribute(string id)
    {
        Id = id;
    }
}
