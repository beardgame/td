using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Fitness;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using Extensions = Bearded.TD.Tiles.Extensions;

namespace Bearded.TD.Game.Generation.Semantic
{
    class LogicalTilemapGenerator
    {
        private readonly Logger logger;

        public LogicalTilemapGenerator(Logger logger)
        {
            this.logger = logger;
        }

        public LogicalTilemap Generate(Random random, int radius)
        {
            var area = Tilemap.TileCountForRadius(radius);
            var areaPerNode = 10 * 10;
            var nodeCount = area / areaPerNode / 2;
            var creviceCount = nodeCount;
            var nodeRadius = ((float) areaPerNode).Sqrted().U() * 0.5f;

            logger.Trace?.Log($"Attempting to generate {nodeCount} nodes");

            var logicalTileMapRadius = 1;
            var logicalTiles = 7;
            while (logicalTiles < nodeCount)
            {
                logicalTileMapRadius++;
                logicalTiles = Tilemap.TileCountForRadius(logicalTileMapRadius);
            }

            var emptyLogicalNodes = logicalTiles - nodeCount;
            var outerRingTotalNodes = logicalTileMapRadius * 6;
            var outerRingActualNodes = outerRingTotalNodes - emptyLogicalNodes;
            var outerRingNodeStep = (float) outerRingActualNodes / outerRingTotalNodes;
            var outerRingOffset = random.Next(outerRingTotalNodes);

            var baseBlueprint = new NodeBlueprint(ImmutableArray.Create<INodeBehavior>(
                new BaseNodeBehavior(),
                new ForceToCenter(),
                new DontBeAdjacentToTag(new NodeTag("spawner")),
                new ClearAllTiles()));
            var spawnerBlueprint = new NodeBlueprint(ImmutableArray.Create<INodeBehavior>(
                new SpawnerNodeBehavior(),
                new ClearAllTiles()));
            var emptyBlueprint = new NodeBlueprint(ImmutableArray.Create<INodeBehavior>(
                new ClearAllTiles()));

            var spawnerFraction = 0.5;
            var spawnerCount = MoreMath.RoundToInt(nodeCount * spawnerFraction);
            var emptyCount = nodeCount - spawnerCount - 1;

            var nodesToPutDown = Enumerable.Repeat(emptyBlueprint, emptyCount)
                .Concat(Enumerable.Repeat(spawnerBlueprint, spawnerCount))
                .Concat(baseBlueprint.Yield())
                .ToList();

            nodesToPutDown.Shuffle(random);


            var logicalNodes = new Tilemap<NodeBlueprint?>(logicalTileMapRadius);

            var tilesThatShouldHaveNodes = Tilemap.EnumerateTilemapWith(logicalTileMapRadius - 1).Concat(
                Tilemap.GetRingCenteredAt(Tile.Origin, logicalTileMapRadius)
                    .Where((_, i) =>
                    {
                        var j = (i + outerRingOffset) % outerRingTotalNodes;
                        var currentStepValue = (int) (j * outerRingNodeStep);
                        var nextStepValue = (int) ((j + 1) * outerRingNodeStep);

                        return currentStepValue != nextStepValue;
                    }));

            foreach (var (tile, nodeBlueprint) in tilesThatShouldHaveNodes.Zip(nodesToPutDown))
            {
                logicalNodes[tile] = nodeBlueprint;
            }

            var normalizedDirections = new[] {Direction.Right, Direction.UpRight, Direction.UpLeft};
            var logicalMacroFeatures = Tilemap.EnumerateTilemapWith(logicalTileMapRadius)
                // this selection won't work for larger numbers of crevices
                .RandomSubset(creviceCount, random)
                .ToDictionary(t => t.Edge(normalizedDirections.RandomElement(random)),
                    _ => new Crevice() as MacroFeature);

            var logicalTilemap = LogicalTilemap.From(logicalNodes, logicalMacroFeatures);

            foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTilemap.Radius))
            {
                var node = logicalTilemap[tile];
                if (node.Blueprint == null)
                    continue;

                var randomValidDirection = Extensions.Directions.Where(d =>
                {
                    var n = tile.Neighbour(d);
                    return logicalTilemap.IsValidTile(n) && logicalTilemap[n].Blueprint != null;
                }).RandomElement(random);

                logicalTilemap.InvertConnectivity(tile.Edge(randomValidDirection));
            }

            logicalTilemap = optimize(logicalTilemap, random);

            return logicalTilemap;
        }

        private LogicalTilemap optimize(LogicalTilemap logicalTilemap, Random random)
        {
            logger.Debug?.Log("Optimising logical tilemap");
            var currentBest = logicalTilemap;
            var currentFitness = fitnessFunction.FitnessOf(currentBest);

            logger.Debug?.Log($"Initial fitness\n{currentFitness}");

            var generatedCount = 500;
            var mutationsPerGeneration = 5;
            var acceptedCount = 0;
            var lastImprovement = -1;

            foreach (var i in Enumerable.Range(0, generatedCount))
            {
                // mutations: connect / disconnect, switch nodes

                var mutated = mutate(currentBest, mutationsPerGeneration, random);

                var mutatedFitness = fitnessFunction.FitnessOf(mutated);

                if (mutatedFitness.Value < currentFitness.Value)
                {
                    currentBest = mutated;
                    currentFitness = mutatedFitness;
                    acceptedCount++;
                    lastImprovement = i;

                    //logger.Debug?.Log($"Accepted mutation {i} with fitness\n{currentFitness}");
                }
            }

            logger.Debug?.Log($"Final fitness:\n{currentFitness}");
            logger.Debug?.Log($"Accepted {acceptedCount} mutations, last was {lastImprovement}/{generatedCount}");


            return currentBest;
        }

        private static readonly FitnessFunction<LogicalTilemap> fitnessFunction = FitnessFunction.From(
            LogicalTilemapFitness.ConnectedComponentsCount,
            LogicalTilemapFitness.DisconnectedCrevices,
            LogicalTilemapFitness.ConnectedTrianglesCount,
            LogicalTilemapFitness.NodeBehaviorFitness,
            LogicalTilemapFitness.ConnectionDegreeHistogramDifference(
                new[] {0, /*1*/ 0.15, /*2*/ 0.2, /*3*/ 0.45, /*4*/ 0.2, 0, 0})
        );

        enum MutationType
        {
            ToggleConnection = 1,
            SwitchNodeBlueprint = 2,
            SwapMacroFeatures = 3,
        }

        private static readonly ImmutableArray<MutationType> mutationTypes =
            Enum.GetValues<MutationType>().ToImmutableArray();


        private LogicalTilemap mutate(LogicalTilemap currentBest, int mutations, Random random)
        {
            var copy = currentBest.Clone();

            var mutationsMade = 0;
            while (mutationsMade < mutations)
            {
                var mutation = mutationTypes.RandomElement<MutationType>(random);

                var success = mutation switch
                {
                    MutationType.ToggleConnection => tryToggleConnection(random, copy),
                    MutationType.SwitchNodeBlueprint => trySwitchBlueprint(random, copy),
                    MutationType.SwapMacroFeatures => trySwapMacroFeatures(random, copy),
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (success)
                    mutationsMade++;
            }

            return copy;
        }

        private bool trySwapMacroFeatures(Random random, LogicalTilemap tilemap)
        {
            var tile = Tilemap.EnumerateTilemapWith(tilemap.Radius).RandomElement(random);
            var node = tilemap[tile];
            if (node.MacroFeatures.IsEmpty)
                return false;

            var direction1 = node.MacroFeatures.Keys.RandomElement(random);
            var direction2 = Extensions.Directions.Except(direction1.Yield()).RandomElement(random);

            tilemap.SwitchMacroFeatures(tile, direction1, direction2);
            return true;
        }

        private bool tryToggleConnection(Random random, LogicalTilemap tilemap)
        {
            return tryCallOnCollectedTilesWithBlueprint(random, tilemap, tilemap.InvertConnectivity);
        }

        private bool trySwitchBlueprint(Random random, LogicalTilemap tilemap)
        {
            return tryCallOnCollectedTilesWithBlueprint(
                random, tilemap, e => tilemap.SwapNodes(e.AdjacentTiles));
        }

        private bool tryCallOnCollectedTilesWithBlueprint(Random random, LogicalTilemap tilemap,
            Action<TileEdge> action)
        {
            var tile = Tilemap.EnumerateTilemapWith(tilemap.Radius).RandomElement(random);
            if (tilemap[tile].Blueprint == null)
                return false;
            var direction = Extensions.Directions.RandomElement(random);
            var neighborTile = tile.Neighbour(direction);
            if (!tilemap.IsValidTile(neighborTile) || tilemap[neighborTile].Blueprint == null)
                return false;

            action(tile.Edge(direction));
            return true;
        }
    }
}
