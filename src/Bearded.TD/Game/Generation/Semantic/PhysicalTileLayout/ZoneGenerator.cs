using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Commands.LevelGeneration;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

sealed class ZoneGenerator
{
    public CommandFactory GenerateZones(List<TiledFeature> tiledFeatures)
    {
        var zonesForNode = generateZones(tiledFeatures);
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
        IEnumerable<TiledFeature> tiledFeatures)
    {
        // TODO: figure out what to do with the tiles between nodes.
        return tiledFeatures
            .OfType<TiledFeature.Node>()
            .ToImmutableDictionary(
                node => node.NodeFeature,
                node => new Zone(Id<Zone>.Invalid, node.Tiles.ToImmutableArray()));
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