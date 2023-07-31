using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features;

sealed record PlacedNode(
    Node? Blueprint,
    IBiome? Biome,
    Directions ConnectedTo,
    ImmutableDictionary<Direction, MacroFeature> MacroFeatures);
