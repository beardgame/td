using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    abstract class TiledFeature
    {
        public PhysicalFeature Feature { get; }
        public ImmutableHashSet<Tile> Tiles { get; }

        private TiledFeature(PhysicalFeature feature, ImmutableHashSet<Tile> tiles)
        {
            Feature = feature;
            Tiles = tiles;
        }

        public static With<TFeature> From<TFeature>(TFeature feature, IEnumerable<Tile> tiles)
            where TFeature : PhysicalFeature
            => new(feature, tiles.ToImmutableHashSet());

        public sealed class With<TFeature> : TiledFeature
            where TFeature : PhysicalFeature
        {
            public new TFeature Feature { get; }

            public With(TFeature feature, ImmutableHashSet<Tile> tiles) : base(feature, tiles)
            {
                Feature = feature;
            }
        }

        public void Deconstruct(out PhysicalFeature feature, out ImmutableHashSet<Tile> tiles)
        {
            (feature, tiles) = (Feature, Tiles);
        }
    }
}
