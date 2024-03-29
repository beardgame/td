using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.Fitness;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using static Bearded.TD.Game.Generation.Semantic.Logical.LogicalTilemapMutations;
using Extensions = Bearded.TD.Tiles.Extensions;

namespace Bearded.TD.Game.Generation.Semantic.Logical;

sealed class LogicalTilemapGenerator
{
    private readonly Logger logger;

    private sealed record Settings(
        int AreaPerNode = 10 * 10,
        float NodeFillRatio = 0.5f,
        float CreviceToNodeRatio = 1);

    private static readonly ImmutableArray<ILogicalTilemapMutation> mutations =
        ImmutableArray.Create<ILogicalTilemapMutation>(
            new SwapMacroFeatures(),
            new ToggleConnection(),
            new SwapNodes(),
            new SwapBiomes());

    private static readonly FitnessFunction<LogicalTilemap> fitnessFunction = FitnessFunction.From(
        LogicalTilemapFitness.ConnectedComponentsCount,
        LogicalTilemapFitness.DisconnectedCrevices,
        LogicalTilemapFitness.ConnectedTrianglesCount,
        LogicalTilemapFitness.CriticalConnectionCount,
        LogicalTilemapFitness.NodeBehaviorFitness,
        LogicalTilemapFitness.ConnectionDegreeHistogramDifference(
            new[] {0, /*1*/ 0.15, /*2*/ 0.2, /*3*/ 0.45, /*4*/ 0.2, 0, 0}),
        LogicalTilemapFitness.BiomeComponentCount
    );

    private readonly LogicalTilemapOptimizer optimizer;

    public LogicalTilemapGenerator(Logger logger)
    {
        this.logger = logger;
        optimizer = new LogicalTilemapOptimizer(logger, mutations, fitnessFunction);
    }

    public LogicalTilemap Generate(LevelGenerationParameters parameters, Random random)
    {
        var logicalTilemap =
            generateInitialTilemap(parameters.Radius, parameters.Nodes, parameters.Biomes, new Settings(), random);

        logicalTilemap = optimizer.Optimize(logicalTilemap, random);

        return logicalTilemap;
    }

    private LogicalTilemap generateInitialTilemap(
        int radius, NodeGroup nodes, IEnumerable<IBiome> biomes, Settings settings, Random random)
    {
        var (areaPerNode, nodeFillRatio, creviceToNodeRatio) = settings;
        var totalArea = Tilemap.TileCountForRadius(radius);
        var nodeCount = MoreMath.FloorToInt(nodeFillRatio * totalArea / areaPerNode);

        var tilemapRadius = lowestRadiusFittingTileCount(nodeCount);

        var nodesToPutDown = new DeterministicNodeChooser(logger).ChooseNodes(nodes, nodeCount).ToImmutableArray();
        var tilemap = generateInitialNodes(tilemapRadius, nodesToPutDown, random);

        var biomesToPutDown = chooseBiomes(biomes, nodeCount).ToImmutableArray();
        var biomeMap = generateInitialBiomes(tilemap, biomesToPutDown, random);

        var creviceCount = MoreMath.FloorToInt(creviceToNodeRatio * nodeCount);
        var macroFeatures = generateInitialMacroFeatures(tilemapRadius, creviceCount, random);

        var logicalTilemap = LogicalTilemap.From(tilemap, biomeMap, macroFeatures);

        invertRandomConnectionForEveryTile(logicalTilemap, random);

        return logicalTilemap;
    }

    private static int lowestRadiusFittingTileCount(int nodeCount)
    {
        var logicalTileMapRadius = 1;
        var logicalTiles = 7;
        while (logicalTiles < nodeCount)
        {
            logicalTileMapRadius++;
            logicalTiles = Tilemap.TileCountForRadius(logicalTileMapRadius);
        }

        return logicalTileMapRadius;
    }

    private static Tilemap<Node?> generateInitialNodes(
        int tilemapRadius, ICollection<Node> nodesToPutDown, Random random)
    {
        var tilesThatShouldHaveNodes =
            chooseTilesThatShouldHaveNodes(tilemapRadius, nodesToPutDown.Count, random).ToList();
        tilesThatShouldHaveNodes.Shuffle(random);
        var tilemap = new Tilemap<Node?>(tilemapRadius);
        foreach (var (tile, nodeBlueprint) in tilesThatShouldHaveNodes.Zip(nodesToPutDown))
        {
            tilemap[tile] = nodeBlueprint;
        }

        return tilemap;
    }

    private static IEnumerable<Tile> chooseTilesThatShouldHaveNodes(
        int tilemapRadius, int nodeCount, Random random)
    {
        var tileCount = Tilemap.TileCountForRadius(tilemapRadius);
        var emptyLogicalNodes = tileCount - nodeCount;
        var outerRingTotalNodes = tilemapRadius * 6;
        var outerRingActualNodes = outerRingTotalNodes - emptyLogicalNodes;
        var outerRingNodeStep = (float) outerRingActualNodes / outerRingTotalNodes;
        var outerRingOffset = random.Next(outerRingTotalNodes);

        var interior = Tilemap.EnumerateTilemapWith(tilemapRadius - 1);
        var partialOuterRing = Tilemap.GetRingCenteredAt(Tile.Origin, tilemapRadius)
            .Where((_, i) =>
            {
                var j = (i + outerRingOffset) % outerRingTotalNodes;
                var currentStepValue = (int) (j * outerRingNodeStep);
                var nextStepValue = (int) ((j + 1) * outerRingNodeStep);

                return currentStepValue != nextStepValue;
            });

        return interior.Concat(partialOuterRing);
    }

    private static IEnumerable<IBiome> chooseBiomes(IEnumerable<IBiome> availableBiomes, int nodeCount)
    {
        var array = availableBiomes.ToImmutableArray();
        for (var i = 0; i < nodeCount; i++)
        {
            yield return array[i % array.Length];
        }
    }

    private static Tilemap<IBiome?> generateInitialBiomes(
        Tilemap<Node?> tilemap, IEnumerable<IBiome> biomes, Random random)
    {
        var shuffledBiomes = biomes.Shuffled(random).ToImmutableArray();
        var result = new Tilemap<IBiome?>(tilemap.Radius);
        var i = 0;
        foreach (var tile in tilemap)
        {
            if (tilemap[tile] is null)
            {
                continue;
            }

            result[tile] = shuffledBiomes[i++];
        }

        return result;
    }

    private static Dictionary<TileEdge, MacroFeature> generateInitialMacroFeatures(
        int tilemapRadius, int creviceCount, Random random)
    {
        var normalizedDirections = new[] {Direction.Right, Direction.UpRight, Direction.UpLeft};
        // this selection won't work for larger numbers of crevices
        var macroFeatures = chooseRandomTiles(tilemapRadius, creviceCount, random)
            .ToDictionary(
                t => t.Edge(normalizedDirections.RandomElement(random)),
                _ => new Crevice() as MacroFeature);
        return macroFeatures;
    }

    private static IEnumerable<Tile> chooseRandomTiles(int tilemapRadius, int tileCount, Random random)
    {
        return Tilemap.EnumerateTilemapWith(tilemapRadius).RandomSubset(tileCount, random);
    }

    private static void invertRandomConnectionForEveryTile(LogicalTilemap logicalTilemap, Random random)
    {
        foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTilemap.Radius))
        {
            var node = logicalTilemap[tile];
            if (node.Blueprint == null)
            {
                continue;
            }

            var randomValidDirection = Extensions.Directions.Where(d =>
            {
                var n = tile.Neighbor(d);
                return logicalTilemap.IsValidTile(n) && logicalTilemap[n].Blueprint != null;
            }).RandomElement(random);

            logicalTilemap.InvertConnectivity(tile.Edge(randomValidDirection));
        }
    }
}
