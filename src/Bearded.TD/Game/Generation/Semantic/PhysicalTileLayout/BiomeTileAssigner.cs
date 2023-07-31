using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

sealed class BiomeTileAssigner
{
    private readonly int tilemapRadius;

    public BiomeTileAssigner(int tilemapRadius)
    {
        this.tilemapRadius = tilemapRadius;
    }

    public Tilemap<IBiome> AssignBiomes(IEnumerable<TiledFeature> tiledFeatures)
    {
        var tilemap = new Tilemap<IBiome>(tilemapRadius);
        var queue = new Queue<Tile>();

        foreach (var feature in tiledFeatures)
        {
            if (feature.Feature is not PhysicalFeature.Node node)
            {
                continue;
            }

            foreach (var tile in feature.Tiles)
            {
                tilemap[tile] = node.Biome;
            }

            var borderOutside = feature.Tiles
                .SelectMany(t => t.PossibleNeighbours())
                .Where(tilemap.IsValidTile)
                .Where(t => tilemap[t] == default)
                .Distinct();
            foreach (var tile in borderOutside)
            {
                queue.Enqueue(tile);
            }
        }

        while (queue.TryDequeue(out var tile))
        {
            if (tilemap[tile] != default)
            {
                continue;
            }

            var neighbors = tile.PossibleNeighbours().Where(tilemap.IsValidTile).ToImmutableArray();
            var mostCommonNeighboringBiome = neighbors
                .GroupBy(t => tilemap[t])
                .Where(group => group.Key != default)
                .MaxBy(group => group.Count())
                .Key;
            tilemap[tile] = mostCommonNeighboringBiome;

            foreach (var t in neighbors.Where(t => tilemap[t] == default))
            {
                queue.Enqueue(t);
            }
        }

        return tilemap;
    }
}
