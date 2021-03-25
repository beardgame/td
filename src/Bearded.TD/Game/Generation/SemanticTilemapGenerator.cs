using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Generation.Fitness;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.World;
using static Bearded.TD.Game.Debug.LevelDebugMetadata;
using Tile = Bearded.TD.Tiles.Tile;

namespace Bearded.TD.Game.Generation
{
    record LogicalNode(int MinimumConnectionCount, int MaximumConnectionCount, Directions ConnectedTo)
    {
        public LogicalNode WithConnection(Direction d)
            => this with {ConnectedTo = ConnectedTo.And(d)};

        public LogicalNode WithoutConnection(Direction d)
            => this with {ConnectedTo = ConnectedTo.Except(d)};

        public LogicalNode WithInverseConnection(Direction d)
            => this with {ConnectedTo = ConnectedTo ^ d.ToDirections()};
    }

    sealed class SemanticTilemapGenerator : ITilemapGenerator
    {
        private readonly Logger logger;
        private readonly LevelDebugMetadata metadata;

        public SemanticTilemapGenerator(Logger logger, LevelDebugMetadata metadata)
        {
            this.logger = logger;
            this.metadata = metadata;
        }

        class Node
        {
            public Position2 Position { get; set; }
            public Unit Radius { get; set; }

            public Angle Orientation { get; set; }

            public List<PolarPosition> ConnectionPoints { get; } = new();

            public Node(Position2 position, Unit radius, IEnumerable<PolarPosition> connectionPoints)
            {
                Position = position;
                Radius = radius;
                ConnectionPoints.AddRange(connectionPoints);
            }
        }

        public Tilemap<TileGeometry> Generate(int radius, int seed)
        {
            logger.Debug?.Log($"Started generating map with radius {radius} and seed {seed}");
            var timer = Stopwatch.StartNew();

            var random = new Random(seed);

            var area = Tilemap.TileCountForRadius(radius);
            var areaPerNode = 12 * 12;
            var nodeCount = area / areaPerNode;
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

            var logicalTilemap = new Tilemap<LogicalNode?>(logicalTileMapRadius);
            foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTileMapRadius - 1).Concat(
                Tilemap.GetRingCenteredAt(Tile.Origin, logicalTileMapRadius)
                    .Where((_, i) =>
                    {
                        var currentStepValue = (int) (i * outerRingNodeStep);
                        var nextStepValue = (int) ((i + 1) * outerRingNodeStep);

                        return currentStepValue != nextStepValue;
                    })
            ))
            {
                logicalTilemap[tile] = new LogicalNode(1, 4, Directions.None);
            }

            foreach (var tile in logicalTilemap)
            {
                var node = logicalTilemap[tile];
                if (node == null)
                    continue;

                var randomValidDirection = Tiles.Extensions.Directions.Where(d =>
                {
                    var n = tile.Neighbour(d);
                    return logicalTilemap.IsValidTile(n) && logicalTilemap[n] != null;
                }).RandomElement(random);

                logicalTilemap[tile] = node.WithConnection(randomValidDirection);

                var neighbourTile = tile.Neighbour(randomValidDirection);
                var neighborNode = logicalTilemap[neighbourTile];
                logicalTilemap[neighbourTile] = neighborNode.WithConnection(randomValidDirection.Opposite());
            }

            logicalTilemap = optimize(logicalTilemap, random);

            foreach (var tile in logicalTilemap)
            {
                var node = logicalTilemap[tile];
                if (node == null)
                    continue;
                var center = Position2.Zero + Level.GetPosition(tile).NumericValue * nodeRadius * 2;
                metadata.Add(new Disk(
                    center,
                    nodeRadius, Color.Bisque * 0.1f
                ));
                foreach (var connectedDirection in node.ConnectedTo.Enumerate())
                {
                    metadata.Add(new LineSegment(center, center + connectedDirection.Vector() * nodeRadius,
                        Color.Lime * 0.25f));
                }
            }

            /*
            var unitRadius = radius * HexagonWidth.U();

            var nodes = Enumerable.Range(0, nodeCount).Select(_ =>
            {
                var dir = Direction2.FromDegrees(random.NextFloat(360));
                var d = random.NextFloat().Sqrted() * unitRadius;
                var p = Position2.Zero + dir * d;

                var r = random.NormalFloat(1, 0.2f).Clamped(0.75f, 1.2f) * nodeRadius;

                var connectionPointCountMean = r.NumericValue / 2;
                var connectionPointCount = MoreMath.RoundToInt(random.NormalFloat(connectionPointCountMean, 1))
                    .Clamped((int)(connectionPointCountMean * 0.75f), (int)(connectionPointCountMean * 1.5f));

                var connectionPointInterval = Angle.FromDegrees(360f / connectionPointCount);
                var connectionPoints = Enumerable.Range(0, connectionPointCount)
                    .Select(i =>
                    {
                        var minAngle = Direction2.Zero + connectionPointInterval * i;
                        var angle = minAngle + random.NextFloat(0.8f) * connectionPointInterval;

                        return new PolarPosition(r.NumericValue - HexagonWidth, angle);
                    });

                return new Node(p, r, connectionPoints);
            }).ToList();

            relax(nodes, radius.U());

            foreach (var node in nodes)
            {
                metadata.Add(new Circle(node.Position, node.Radius, 0.3.U(), Color.Cyan * 0.5f));
                foreach (var connectionPoint in node.ConnectionPoints)
                {
                    var p = node.Position + new Difference2(
                        new PolarPosition(connectionPoint.R, connectionPoint.Angle + node.Orientation).ToVector2()
                        );

                    metadata.Add(new Disk(p, 1.U(), Color.Red * 0.5f));
                }
            }


            var areas = nodes.ToDictionary(n => n, _ => new List<Tile>());

            foreach (var tile in Tilemap.EnumerateTilemapWith(radius))
            {
                var tilePosition = Level.GetPosition(tile);

                var node = nodes.MinBy(n => ((tilePosition - n.Position).Length - n.Radius).NumericValue);

                areas[node].Add(tile);
            }

            foreach (var (_, tiles) in areas)
            {
                metadata.Add(new AreaBorder(TileAreaBorder.From(tiles), Color.Beige * 0.5f));
            }
            */

            logger.Debug?.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

            return new Tilemap<TileGeometry>(radius, _ => new TileGeometry(TileType.Floor, 1, Unit.Zero));
        }

        private Tilemap<LogicalNode?> optimize(Tilemap<LogicalNode?> logicalTilemap, Random random)
        {
            logger.Debug?.Log("Optimising logical tilemap");
            var currentBest = logicalTilemap;
            var currentFitness = fitnessFunction.FitnessOf(currentBest);

            logger.Debug?.Log($"Initial fitness\n{currentFitness}");

            var generatedCount = 100;
            var acceptedCount = 0;
            var lastImprovement = -1;

            foreach (var i in Enumerable.Range(0, generatedCount))
            {
                // mutations: connect / disconnect, switch nodes

                var mutated = mutate(currentBest, 3, random);

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

        private FitnessFunction<Tilemap<LogicalNode?>> fitnessFunction = FitnessFunction.From(
            LogicalTilemapFitness.ConnectedComponentsCount,
            LogicalTilemapFitness.ConnectedTrianglesCount,
            LogicalTilemapFitness.ConnectionDegreeHistogramDifference(
                new[] {0, /*1*/ 0.15, /*2*/ 0.2, /*3*/ 0.45, /*4*/ 0.2, 0, 0})
        );

        private Tilemap<LogicalNode?> mutate(Tilemap<LogicalNode?> currentBest, int mutations, Random random)
        {
            var copy = currentBest.Clone();

            foreach (var _ in Enumerable.Range(0, mutations))
            {
                var tile = copy.RandomElement(random);
                if (copy[tile] == null)
                    continue;
                var direction = Tiles.Extensions.Directions.RandomElement(random);
                var neighborTile = tile.Neighbour(direction);
                if (!copy.IsValidTile(neighborTile) || copy[neighborTile] == null)
                    continue;

                copy[tile] = copy[tile].WithInverseConnection(direction);
                copy[neighborTile] = copy[neighborTile].WithInverseConnection(direction.Opposite());
            }

            return copy;
        }

        private void relax(List<Node> nodes, Unit radius)
        {
            var allPairs =
                (from n1 in nodes
                    from n2 in nodes.TakeWhile(n => n != n1)
                    select (n1, n2)).ToList();

            foreach (var _ in Enumerable.Range(0, 100))
            {
                foreach (var (n1, n2) in allPairs)
                {
                    var diff = n1.Position - n2.Position;
                    var dSquared = diff.LengthSquared;

                    var maxD = n1.Radius + n2.Radius;
                    var maxDSquared = maxD.Squared;

                    if (maxDSquared < dSquared)
                        continue;

                    var forceMagnitude = (maxDSquared.NumericValue - dSquared.NumericValue).U() * 0.01f;

                    var forceOnN1 = diff / dSquared.Sqrt() * forceMagnitude;

                    n1.Position += forceOnN1;
                    n2.Position -= forceOnN1;
                }

                foreach (var node in nodes)
                {
                    foreach (var direction in Tiles.Extensions.Directions)
                    {
                        var lineNormal = direction.CornerAfter();
                        var lineDistance = HexagonDistanceY * radius - node.Radius;

                        var projection = Vector2.Dot(lineNormal, node.Position.NumericValue).U();

                        if (projection < lineDistance)
                            continue;

                        var forceMagnitude =
                            (lineDistance.NumericValue.Squared() - projection.NumericValue.Squared()).U() * 0.1f;

                        node.Position += lineNormal * forceMagnitude;
                    }
                }
            }
        }
    }
}
