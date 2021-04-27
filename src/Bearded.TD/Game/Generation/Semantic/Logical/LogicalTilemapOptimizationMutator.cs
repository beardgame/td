using System;
using Bearded.TD.Tiles;
using Bearded.Utilities.Linq;
using Extensions = Bearded.TD.Tiles.Extensions;

namespace Bearded.TD.Game.Generation.Semantic.Logical
{
    static class LogicalTilemapOptimizationMutator
    {
        public static bool TryCallOnConnectedTilesWithBlueprint(
            LogicalTilemap tilemap, Action<TileEdge> action, Random random)
        {
            if (tryFindConnectedEdgeBetweenBlueprints(tilemap, random, out var tileEdge))
            {
                action(tileEdge);
                return true;
            }

            return false;
        }

        private static bool tryFindConnectedEdgeBetweenBlueprints(
            LogicalTilemap tilemap, Random random, out TileEdge tileEdge)
        {
            tileEdge = default;

            var randomTile = Tilemap.EnumerateTilemapWith(tilemap.Radius).RandomElement(random);
            if (tilemap[randomTile].Blueprint == null)
            {
                return false;
            }

            var randomDirection = Extensions.Directions.RandomElement(random);
            var neighborTile = randomTile.Neighbour(randomDirection);
            if (!tilemap.IsValidTile(neighborTile) || tilemap[neighborTile].Blueprint == null)
            {
                return false;
            }

            tileEdge = randomTile.Edge(randomDirection);
            return true;
        }
    }
}
