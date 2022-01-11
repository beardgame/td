using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

sealed class ZoneGenerator
{
    public CommandFactory GenerateZones(List<TiledFeature> tiledFeatures, Tilemap<TileGeometry> tilemap)
    {
        var zonesForNode = generateZones(tiledFeatures, tilemap);
        var connectionsBetweenNodes = listConnectedNodes(tiledFeatures);
        return game =>
        {
            var zonesWithIds = patchWithIds(game, zonesForNode);
            var connectionsBetweenZones =
                connectionsBetweenNodes.SelectBoth(node => zonesWithIds[node]).ToImmutableArray();
            return DefineZones.Command(game, zonesWithIds.Values.ToImmutableArray(), connectionsBetweenZones);
        };
    }

    private static ImmutableDictionary<PhysicalFeature.Node, Zone> generateZones(
        IEnumerable<TiledFeature> tiledFeatures, Tilemap<TileGeometry> tilemap)
    {
        var nodes = tiledFeatures.OfType<TiledFeature.Node>().ToImmutableArray();
        var nodeTiles = nodes.SelectMany(node => node.Tiles).ToImmutableHashSet();

        return nodes.ToImmutableDictionary(
                node => node.NodeFeature,
                node => new Zone(Id<Zone>.Invalid, generateTilesForZone(node.Tiles.ToImmutableArray())));

        ImmutableArray<Tile> generateTilesForZone(ImmutableArray<Tile> startingTiles)
        {
            var tiles = new HashSet<Tile>(startingTiles);
            // Add the adjacent unassigned tiles type by type, since we want to avoid having situations where we also
            // count connection tiles between different zones.
            adjacentUnassignedTiles(startingTiles, TileType.Floor).ForEach(t => tiles.Add(t));
            adjacentUnassignedTiles(startingTiles, TileType.Crevice).ForEach(t => tiles.Add(t));
            return tiles.ToImmutableArray();
        }

        IEnumerable<Tile> adjacentUnassignedTiles(ImmutableArray<Tile> startingTiles, TileType tileType)
        {
            var queue = new Queue<Tile>(startingTiles);
            var seen = new HashSet<Tile>(startingTiles);
            while (queue.TryDequeue(out var t))
            {
                var unassignedFloorNeighbors = t.PossibleNeighbours()
                    .Where(n => !seen.Contains(n)
                        && !nodeTiles.Contains(n)
                        && tilemap[n].Type == tileType);
                foreach (var n in unassignedFloorNeighbors)
                {
                    yield return n;
                    queue.Enqueue(n);
                    seen.Add(n);
                }
            }
        }
    }

    private static ImmutableArray<(PhysicalFeature.Node From, PhysicalFeature.Node To)> listConnectedNodes(
        IEnumerable<TiledFeature> tiledFeatures)
    {
        return tiledFeatures
            .Select(t => t.Feature)
            .OfType<PhysicalFeature.Connection>()
            .Select(connection => (From: connection.From.Feature, To: connection.To.Feature))
            .WhereBoth(feature => feature is PhysicalFeature.Node)
            .SelectBoth(feature => (PhysicalFeature.Node) feature)
            .ToImmutableArray();
    }

    private static ImmutableDictionary<PhysicalFeature.Node, Zone> patchWithIds(
        GameInstance game, IDictionary<PhysicalFeature.Node, Zone> zones)
    {
        return zones.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value with { Id = game.Ids.GetNext<Zone>() });
    }
}
