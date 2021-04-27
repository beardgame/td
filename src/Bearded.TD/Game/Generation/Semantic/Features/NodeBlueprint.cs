using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Logical;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    sealed record NodeBlueprint(ImmutableArray<INodeBehavior> Behaviors)
    {
        private ImmutableHashSet<NodeTag>? tags;

        public ImmutableHashSet<NodeTag> Tags => tags ??= Behaviors.SelectMany(b => b.Tags).ToImmutableHashSet();
    }
}
