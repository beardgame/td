using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.Props;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

static partial class TilemapGenerator
{
    private static Tilemap<TileGeometry> generateFeatures(
        int radius,
        IEnumerable<TiledFeature> features,
        LevelGenerationCommandAccumulator commandAccumulator,
        Accumulator<PropHint> propHintAccumulator,
        Random random)
    {
        var tilemap =
            new Tilemap<TileGeometry>(radius, _ => new TileGeometry(TileType.Wall, 1, Unit.Zero));

        foreach (var feature in features)
        {
            switch (feature)
            {
                case TiledFeature.Node node:
                    generateNode(feature, tilemap, node, commandAccumulator, propHintAccumulator, random);
                    break;
                case (PhysicalFeature.Connection, _):
                    generateConnection(feature, tilemap);
                    break;
                case (PhysicalFeature.Crevice, _):
                    generateCrevice(feature, tilemap);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        return tilemap;
    }

    private static void generateNode(
        TiledFeature feature,
        Tilemap<TileGeometry> tilemap,
        TiledFeature.Node node,
        LevelGenerationCommandAccumulator commandAccumulator,
        Accumulator<PropHint> propHintAccumulator,
        Random random)
    {
        var context = NodeGenerationContext.Create(tilemap,
            feature.Tiles,
            node.NodeFeature.Circles,
            node.Connections,
            commandAccumulator,
            propHintAccumulator,
            random);
        foreach (var behavior in node.NodeFeature.Blueprint.Behaviors)
        {
            behavior.Generate(context);
        }

        ensureBuildingsAreOnFloor(context);
        ensureConnectedness(context);
    }

    private static void ensureBuildingsAreOnFloor(NodeGenerationContext context)
    {
        var nonFloorTiles =
            context.Content.BuildingTiles.Where(t => context.Tiles.Get(t).Type != TileType.Floor);
        foreach (var tile in nonFloorTiles)
        {
            context.Tiles.Set(tile, new TileGeometry(TileType.Floor, 0, Unit.Zero));
        }
    }

    private static void ensureConnectedness(NodeGenerationContext context)
    {
        var connections = context.NodeData.Connections;

        if (connections.Length < 2)
            return;

        var tileCount = context.Tiles.All.Count;
        var expensivePathMinimum = tileCount * 2;
        var buildingTileCost = tileCount * expensivePathMinimum;

        var pathfinder = Pathfinder
            .WithTileCosts(tileCost, 1)
            .InArea(context.Tiles.All);

        foreach (var tile in connections.Skip(1))
        {
            var result = pathfinder.FindPath(tile, connections[0]);

            if (result == null)
                throw new InvalidOperationException();

            var currentTile = tile;

            if (result.Cost < expensivePathMinimum)
                continue;

            foreach (var step in result.Path)
            {
                currentTile = currentTile.Neighbor(step);
                setFloor(currentTile);
            }
        }

        foreach (var tile in connections)
        {
            setFloor(tile);
        }

        double? tileCost(Tile tile)
        {
            if (context.Content.BuildingTiles.Contains(tile))
            {
                return buildingTileCost;
            }

            return context.Tiles.Get(tile).Type == TileType.Floor ? 1 : expensivePathMinimum;
        }

        void setFloor(Tile tile) => context.Tiles.Set(tile, new TileGeometry(TileType.Floor, 0, Unit.Zero));
    }

    private static void generateCrevice(TiledFeature feature, Tilemap<TileGeometry> tilemap)
    {
        foreach (var tile in feature.Tiles)
        {
            tilemap[tile] = new TileGeometry(TileType.Crevice, 1, -5.U());
        }
    }

    private static void generateConnection(TiledFeature feature, Tilemap<TileGeometry> tilemap)
    {
        foreach (var tile in feature.Tiles)
        {
            tilemap[tile] = new TileGeometry(TileType.Floor, 1, Unit.Zero);
        }
    }
}
