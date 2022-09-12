using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models;

sealed class NodeBlueprint : INodeBlueprint
{
    public ModAwareId Id { get; }
    public ImmutableArray<INodeBehaviorFactory> Behaviors { get; }
    public Unit? Radius { get; }

    public NodeBlueprint(ModAwareId id, ImmutableArray<INodeBehaviorFactory> behaviors, Unit? radius)
    {
        Id = id;
        Behaviors = behaviors;
        Radius = radius;
    }
}
