using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed record TiledFeature(PhysicalFeature Feature, ImmutableHashSet<Tile> Tiles)
    {
        public static TiledFeature From(PhysicalFeature feature, IEnumerable<Tile> tiles)
            => new(feature, tiles.ToImmutableHashSet());
    }
}
