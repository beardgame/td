using System;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    sealed class NodeTileGenerationContext
    {
        private readonly Tilemap<TileGeometry> tilemap;
        public IArea All { get; }

        public NodeTileGenerationContext(Tilemap<TileGeometry> tilemap, IArea all)
        {
            this.tilemap = tilemap;
            All = all;
        }

        public TileGeometry Get(Tile tile)
        {
            return tilemap[tile];
        }

        public void Set(Tile tile, TileGeometry geometry)
        {
            if (!All.Contains(tile))
            {
                throw new ArgumentException("May not write to tile outside node.", nameof(tile));
            }

            tilemap[tile] = geometry;
        }
    }
}
