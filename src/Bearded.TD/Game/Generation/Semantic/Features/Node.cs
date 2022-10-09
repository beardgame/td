using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.Features;

sealed record Node(ModAwareId Id, ImmutableArray<INodeBehavior> Behaviors, Unit? Radius, bool Explorable)
{
    private ImmutableHashSet<NodeTag>? memoizedTags;

    public ImmutableHashSet<NodeTag> AllTags =>
        memoizedTags ??= Behaviors.SelectMany(b => b.Tags).ToImmutableHashSet();

    public static Node FromBlueprint(INodeBlueprint blueprint) =>
        new(blueprint.Id,
            blueprint.Behaviors.Select(factory => factory.Create()).ToImmutableArray(),
            blueprint.Radius,
            blueprint.Explorable);
}
