using System.Collections.Immutable;
using Bearded.TD.Game.Simulation;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    interface INodeBlueprint : IBlueprint
    {
        ImmutableArray<INodeBehaviorFactory<Node>> Behaviors { get; }
    }
}
