using System.Collections.Immutable;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    record TiledFeature(PhysicalFeature Feature, IArea Tiles)
    {
        public sealed record Node(PhysicalFeature.Node NodeFeature, IArea Tiles, ImmutableArray<Tile> Connections)
            : TiledFeature(NodeFeature, Tiles);
    }
}
