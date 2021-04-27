using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    sealed class NodeGenerationContext
    {
        private readonly Tilemap<TileGeometry> tilemap;

        public ImmutableHashSet<Tile> Tiles { get; }

        // info about connections, etc.

        public NodeGenerationContext(Tilemap<TileGeometry> tilemap, IEnumerable<Tile> tiles)
        {
            this.tilemap = tilemap;
            Tiles = tiles.ToImmutableHashSet();
        }

        public void Set(Tile tile, TileGeometry geometry)
        {
            if (!Tiles.Contains(tile))
                throw new ArgumentException("May not write to tile outside node.", nameof(tile));

            tilemap[tile] = geometry;
        }
    }
}
