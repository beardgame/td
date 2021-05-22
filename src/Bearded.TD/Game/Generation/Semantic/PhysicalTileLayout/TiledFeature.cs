using System.Collections.Generic;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed record TiledFeature(PhysicalFeature Feature, Area Tiles)
    {
        public static TiledFeature From(PhysicalFeature feature, IEnumerable<Tile> tiles)
            => new(feature, Area.From(tiles));
    }
}
