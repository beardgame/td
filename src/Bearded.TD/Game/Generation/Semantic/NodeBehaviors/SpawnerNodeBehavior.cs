using System.Collections.Immutable;
using Bearded.TD.Game.Generation.Semantic.Features;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("spawner")]
    sealed class SpawnerNodeBehavior : NodeBehavior
    {
        public override ImmutableArray<NodeTag> Tags { get; } = ImmutableArray.Create(new NodeTag("spawner"));
    }
}
