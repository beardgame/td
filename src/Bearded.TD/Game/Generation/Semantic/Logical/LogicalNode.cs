using System.Collections.Immutable;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Logical
{
    sealed record LogicalNode(
        NodeBlueprint? Blueprint,
        Directions ConnectedTo,
        ImmutableDictionary<Direction, MacroFeature> MacroFeatures);
}
