using System;
using Bearded.TD.Content.Behaviors;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Factions;

[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(IFactionBehavior<>))]
[MeansImplicitUse]
sealed class FactionBehaviorAttribute : Attribute, IBehaviorAttribute
{
    public string Id { get; }

    public FactionBehaviorAttribute(string id)
    {
        Id = id;
    }
}