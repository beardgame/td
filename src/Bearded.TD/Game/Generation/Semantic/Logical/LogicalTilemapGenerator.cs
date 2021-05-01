using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.Fitness;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using static Bearded.TD.Game.Generation.Semantic.Logical.LogicalTilemapMutations;
using Extensions = Bearded.TD.Tiles.Extensions;

namespace Bearded.TD.Game.Generation.Semantic.Logical
{
    sealed class LogicalTilemapGenerator
    {
        private sealed record Settings(
            int AreaPerNode = 10 * 10,
            float NodeFillRatio = 0.5f,
            float CreviceToNodeRatio = 1,
            float SpawnerFraction = 0.5f);

        private static readonly ImmutableArray<ILogicalTilemapMutation> mutations =
            ImmutableArray.Create<ILogicalTilemapMutation>(
                new SwapMacroFeatures(),
                new ToggleConnection(),
                new SwapBlueprints());

        private static readonly FitnessFunction<LogicalTilemap> fitnessFunction = FitnessFunction.From(
            LogicalTilemapFitness.ConnectedComponentsCount,
            LogicalTilemapFitness.DisconnectedCrevices,
            LogicalTilemapFitness.ConnectedTrianglesCount,
            LogicalTilemapFitness.NodeBehaviorFitness,
            LogicalTilemapFitness.ConnectionDegreeHistogramDifference(
                new[] {0, /*1*/ 0.15, /*2*/ 0.2, /*3*/ 0.45, /*4*/ 0.2, 0, 0})
        );

        private readonly LogicalTilemapOptimizer optimizer;

        public LogicalTilemapGenerator(Logger logger)
        {
            optimizer = new LogicalTilemapOptimizer(logger, mutations, fitnessFunction);
        }

        public LogicalTilemap Generate(Random random, int radius)
        {
            var logicalTilemap = generateInitialTilemap(radius, new Settings(), random);

            logicalTilemap = optimizer.Optimize(logicalTilemap, random);

            return logicalTilemap;
        }

        private static LogicalTilemap generateInitialTilemap(int radius, Settings settings, Random random)
        {
            var (areaPerNode, nodeFillRatio, creviceToNodeRatio, spawnerFraction) = settings;
            var totalArea = Tilemap.TileCountForRadius(radius);
            var nodeCount = MoreMath.FloorToInt(nodeFillRatio * totalArea / areaPerNode);

            var tilemapRadius = lowestRadiusFittingTileCount(nodeCount);

            var nodesToPutDown = chooseNodes(nodeCount, spawnerFraction).ToList();
            var tilemap = generateInitialNodes(tilemapRadius, nodesToPutDown, random);

            var creviceCount = MoreMath.FloorToInt(creviceToNodeRatio * nodeCount);
            var macroFeatures = generateInitialMacroFeatures(tilemapRadius, creviceCount, random);

            var logicalTilemap = LogicalTilemap.From(tilemap, macroFeatures);

            invertRandomConnectionForEveryTile(logicalTilemap, random);

            return logicalTilemap;
        }

        private static Tilemap<Node?> generateInitialNodes(
            int tilemapRadius, ICollection<Node> nodesToPutDown, Random random)
        {
            var tilesThatShouldHaveNodes =
                chooseTilesThatShouldHaveNodes(tilemapRadius, nodesToPutDown.Count, random).ToList();
            tilesThatShouldHaveNodes.Shuffle();
            var tilemap = new Tilemap<Node?>(tilemapRadius);
            foreach (var (tile, nodeBlueprint) in tilesThatShouldHaveNodes.Zip(nodesToPutDown))
            {
                tilemap[tile] = nodeBlueprint;
            }

            return tilemap;
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

        private static IEnumerable<Node> chooseNodes(int nodeCount, float spawnerFraction)
        {
            var baseBlueprint = new Node(ModAwareId.Invalid,
                ImmutableArray.Create<INodeBehavior<Node>>(
                    new BaseNodeBehavior(),
                    new ForceToCenter(),
                    new AvoidTagAdjacency(new AvoidTagAdjacency.BehaviorParameters(new NodeTag("spawner"))),
                    new MakeAllTilesFloor()));
            var spawnerBlueprint = new Node(ModAwareId.Invalid,
                ImmutableArray.Create<INodeBehavior<Node>>(
                    new SpawnerNodeBehavior(),
                    new MakeAllTilesFloor()));
            var emptyBlueprint = new Node(ModAwareId.Invalid,
                ImmutableArray.Create<INodeBehavior<Node>>(
                    new MakeAllTilesFloor()));

            var spawnerCount = MoreMath.RoundToInt(nodeCount * spawnerFraction);
            var emptyCount = nodeCount - spawnerCount - 1;

            return Enumerable.Repeat(emptyBlueprint, emptyCount)
                .Concat(Enumerable.Repeat(spawnerBlueprint, spawnerCount))
                .Concat(baseBlueprint.Yield());
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
                    var n = tile.Neighbour(d);
                    return logicalTilemap.IsValidTile(n) && logicalTilemap[n].Blueprint != null;
                }).RandomElement(random);

                logicalTilemap.InvertConnectivity(tile.Edge(randomValidDirection));
            }
        }
    }
}
