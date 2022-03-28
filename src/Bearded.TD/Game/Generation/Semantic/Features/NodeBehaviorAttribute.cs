using System;
using Bearded.TD.Content.Behaviors;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Generation.Semantic.Features;

[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(INodeBehavior))]
[MeansImplicitUse]
sealed class NodeBehaviorAttribute : Attribute, IBehaviorAttribute
{
    public string Id { get; }

    public NodeBehaviorAttribute(string id)
    {
        Id = id;
    }
}
