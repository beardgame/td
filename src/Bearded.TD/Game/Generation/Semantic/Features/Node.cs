using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Game.Generation.Semantic.Features;

[NodeBehaviorOwner]
sealed record Node(ModAwareId Id, ImmutableArray<INodeBehavior<Node>> Behaviors)
{
    private ImmutableHashSet<NodeTag>? memoizedTags;

    public ImmutableHashSet<NodeTag> AllTags =>
        memoizedTags ??= Behaviors.SelectMany(b => b.Tags).ToImmutableHashSet();

    public static Node FromBlueprint(INodeBlueprint blueprint) =>
        new(blueprint.Id, blueprint.Behaviors.Select(factory => factory.Create()).ToImmutableArray());
}