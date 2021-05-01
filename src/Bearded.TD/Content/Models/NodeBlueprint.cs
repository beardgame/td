using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Features;

namespace Bearded.TD.Content.Models
{
    sealed class NodeBlueprint : INodeBlueprint
    {
        public ModAwareId Id { get; }
        public ImmutableArray<INodeBehaviorFactory<Node>> Behaviors { get; }

        public NodeBlueprint(ModAwareId id, ImmutableArray<INodeBehaviorFactory<Node>> behaviors)
        {
            Id = id;
            Behaviors = behaviors;
        }
    }
}
