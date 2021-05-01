using System.Collections.Immutable;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    sealed record PlacedNode(
        Node? Blueprint,
        Directions ConnectedTo,
        ImmutableDictionary<Direction, MacroFeature> MacroFeatures);
}
