using System.Collections.Immutable;
using Bearded.TD.Game.Simulation;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.Features;

interface INodeBlueprint : IBlueprint
{
    ImmutableArray<INodeBehaviorFactory> Behaviors { get; }
    Unit? Radius { get; }
}
