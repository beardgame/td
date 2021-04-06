using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Bearded.Graphics;
using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Generation.Fitness;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.World;
using static Bearded.TD.Game.Debug.LevelDebugMetadata;
using Extensions = Bearded.TD.Tiles.Extensions;
using Tile = Bearded.TD.Tiles.Tile;

namespace Bearded.TD.Game.Generation
{
    record NodeBlueprint(ImmutableArray<INodeBehavior> Behaviors)
    {
        private ImmutableHashSet<NodeTag>? tags;

        public ImmutableHashSet<NodeTag> Tags => tags ??= Behaviors.SelectMany(b => b.Tags).ToImmutableHashSet();
    }

    interface INodeBehavior
    {
        double GetFitnessPenalty(Tilemap<LogicalNode?> tilemap, Tile nodeTile) => 0;

        IEnumerable<NodeTag> Tags => Enumerable.Empty<NodeTag>();

        void Generate(GenerationContext context)
        {
        }

        // TODO: maybe wanna add ImmutableArray<string> Tags { get; } (not 'string' though)

        string Name => Regex.Replace(GetType().Name, "(Node)?(Behaviou?r)?$", "");
    }

    record NodeTag(string Name);

    class BaseNodeBehavior : INodeBehavior
    {
        public IEnumerable<NodeTag> Tags { get; } = ImmutableArray.Create(new NodeTag("base"));
    }

    class ForceToCenter : INodeBehavior
    {
        public double GetFitnessPenalty(Tilemap<LogicalNode?> tilemap, Tile nodeTile)
        {
            return nodeTile.Radius * 1000;
        }
    }

    class SpawnerNodeBehavior : INodeBehavior
    {
        public IEnumerable<NodeTag> Tags { get; } = ImmutableArray.Create(new NodeTag("spawner"));
    }

    class DontBeAdjacentToTag : INodeBehavior
    {
        private readonly NodeTag tagToAvoid;

        public DontBeAdjacentToTag(NodeTag tagToAvoid)
        {
            this.tagToAvoid = tagToAvoid;
        }

        public double GetFitnessPenalty(Tilemap<LogicalNode?> tilemap, Tile nodeTile)
        {
            var node = tilemap[nodeTile];

            var connectedNodes = Extensions.Directions
                .Where(d => node!.ConnectedTo.Includes(d))
                .Select(nodeTile.Neighbour)
                .Select(t => tilemap[t]);

            return connectedNodes.Count(n => n!.Blueprint.Tags.Contains(tagToAvoid)) * 100;
        }
    }

    class ConnectionCountNodeBehaviour : INodeBehavior
    {
        public FitnessFunction<(Tilemap<LogicalNode?> Tilemap, Tile NodeTile)>? DonateFitnessFunction()
        {
            throw new NotImplementedException();
        }

        public void Generate(GenerationContext context)
        {
            throw new NotImplementedException();
        }
    }

    class GenerationContext
    {
        // easy read and write access to node's tiles (doesn't allow changing other tiles)
        // info about connections, etc.


    }

    record LogicalNode(NodeBlueprint Blueprint, Directions ConnectedTo)
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
            public LogicalNode Logical { get; }

            public Position2 Position { get; set; }
            public Unit Radius { get; set; }

            public Angle Orientation { get; set; }

            public List<PolarPosition> ConnectionPoints { get; } = new();

            public Node(Position2 position, Unit radius, IEnumerable<PolarPosition> connectionPoints, LogicalNode logical)
            {
                Position = position;
                Radius = radius;
                Logical = logical;
                ConnectionPoints.AddRange(connectionPoints);
            }
        }

        class Connection
        {
            public Node Node1 { get; }
            public Node Node2 { get; }

            public Connection(Node node1, Node node2)
            {
                Node1 = node1;
                Node2 = node2;
            }
        }

        public Tilemap<TileGeometry> Generate(int radius, int seed)
        {
            logger.Debug?.Log($"Started generating map with radius {radius} and seed {seed}");
            var timer = Stopwatch.StartNew();

            var random = new Random(seed);

            var area = Tilemap.TileCountForRadius(radius);
            var areaPerNode = 10 * 10;
            var nodeCount = area / areaPerNode / 2;
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

            var baseBlueprint = new NodeBlueprint(ImmutableArray.Create<INodeBehavior>(new BaseNodeBehavior(), new ForceToCenter(), new DontBeAdjacentToTag(new NodeTag("spawner"))));
            var spawnerBlueprint = new NodeBlueprint(ImmutableArray.Create<INodeBehavior>(new SpawnerNodeBehavior()));
            var emptyBlueprint =  new NodeBlueprint(ImmutableArray<INodeBehavior>.Empty);

            var spawnerFraction = 0.5;
            var spawnerCount = MoreMath.RoundToInt(nodeCount * spawnerFraction);
            var emptyCount = nodeCount - spawnerCount - 1;

            var nodesToPutDown = Enumerable.Repeat(emptyBlueprint, emptyCount)
                .Concat(Enumerable.Repeat(spawnerBlueprint, spawnerCount))
                .Concat(baseBlueprint.Yield())
                .ToList();

            nodesToPutDown.Shuffle(random);


            var logicalTilemap = new Tilemap<LogicalNode?>(logicalTileMapRadius);

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
                logicalTilemap[tile] = new LogicalNode(nodeBlueprint, Directions.None);
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
                    nodeRadius, Color.Bisque * 0.05f
                ));
                foreach (var connectedDirection in node.ConnectedTo.Enumerate())
                {
                    metadata.Add(new LineSegment(center, center + connectedDirection.Vector() * nodeRadius,
                        Color.Lime * 0.1f));
                }
            }

            var nodes = new List<Node>();
            var nodeDictionary = new Dictionary<Tile, Node>();
            foreach (var tile in logicalTilemap)
            {
                var node = logicalTilemap[tile];
                if (node == null)
                    continue;

                var center = Position2.Zero + Level.GetPosition(tile).NumericValue * nodeRadius * 2;

                var n = new Node(center, nodeRadius * random.NextFloat(0.75f, 1.2f), Enumerable.Empty<PolarPosition>(), node);
                nodes.Add(n);
                nodeDictionary.Add(tile, n);
            }

            var connections = createNodeConnections(logicalTilemap, nodeDictionary);



            relax(nodes, connections, radius.U());

            foreach (var node in nodes)
            {
                metadata.Add(new LevelDebugMetadata.Circle(node.Position, node.Radius, 0.3.U(), Color.Cyan * 0.5f));
                foreach (var connectionPoint in node.ConnectionPoints)
                {
                    var p = node.Position + new Difference2(
                        new PolarPosition(connectionPoint.R, connectionPoint.Angle + node.Orientation).ToVector2()
                        );

                    metadata.Add(new Disk(p, 1.U(), Color.Red * 0.5f));
                }

                foreach (var (behavior, i) in node.Logical.Blueprint.Behaviors.Indexed())
                {
                    metadata.Add(new Text(
                        node.Position + new Difference2(0, i),
                        behavior.Name ?? "",
                        Color.Cyan * 0.5f, 0, 1.U()
                        ));
                }
            }

            foreach (var connection in connections)
            {
                metadata.Add(new LineSegment(
                    connection.Node1.Position,
                    connection.Node2.Position,
                    Color.Azure * 0.5f
                    ));
            }


            var nodeAreas = nodes.ToDictionary(n => n, _ => new HashSet<Tile>());

            foreach (var tile in Tilemap.EnumerateTilemapWith(radius))
            {
                var tilePosition = Level.GetPosition(tile);

                var node = nodes.MinBy(n => ((tilePosition - n.Position).Length - n.Radius).NumericValue);

                var distanceToNodeSquared = (tilePosition - node.Position).LengthSquared;

                if (distanceToNodeSquared > node.Radius.Squared)
                    continue;

                nodeAreas[node].Add(tile);
            }

            foreach (var nodeArea in nodeAreas.Values)
            {
                erode(nodeArea);
            }

            var connectionAreas = new Dictionary<Connection, HashSet<Tile>>();

            foreach (var connection in connections)
            {
                var areaFrom = nodeAreas[connection.Node1];
                var areaTo = nodeAreas[connection.Node2];

                var rayCaster = new LevelRayCaster();
                rayCaster.StartEnumeratingTiles(new Ray(connection.Node1.Position, connection.Node2.Position - connection.Node1.Position));

                var connectionArea = new HashSet<Tile>();

                foreach (var tile in rayCaster)
                {
                    if (areaFrom.Contains(tile))
                        continue;

                    if (areaTo.Contains(tile))
                        break;

                    connectionArea.Add(tile);
                }
                connectionAreas.Add(connection, connectionArea);
            }

            foreach (var (_, tiles) in nodeAreas)
            {
                metadata.Add(new AreaBorder(TileAreaBorder.From(tiles), Color.Beige * 0.5f));
            }

            foreach (var (_, tiles) in connectionAreas)
            {
                metadata.Add(new AreaBorder(TileAreaBorder.From(tiles), Color.IndianRed * 0.5f));
            }


            logger.Debug?.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

            var finalTilemap = new Tilemap<TileGeometry>(radius, _ => new TileGeometry(TileType.Wall, 1, Unit.Zero));

            foreach (var tile in nodeAreas.Values.Concat(connectionAreas.Values).SelectMany(x => x))
            {
                finalTilemap[tile] = new TileGeometry(TileType.Floor, 1, Unit.Zero);
            }

            return finalTilemap;
        }

        private void erode(HashSet<Tile> area)
        {
            var tilesToErode = area.Where(n => !n.PossibleNeighbours().All(area.Contains)).ToList();
            foreach (var tileToErode in tilesToErode)
            {
                area.Remove(tileToErode);
            }
        }

        private static List<Connection> createNodeConnections(Tilemap<LogicalNode?> logicalTilemap, Dictionary<Tile, Node> nodes)
        {
            var connections = new List<Connection>();
            foreach (var tile in logicalTilemap)
            {
                var node = logicalTilemap[tile];
                if (node == null)
                    continue;

                tryDirection(tile, node, Direction.Right);
                tryDirection(tile, node, Direction.UpRight);
                tryDirection(tile, node, Direction.UpLeft);
            }

            return connections;

            void tryDirection(Tile tile, LogicalNode node, Direction dir)
            {
                if (!node.ConnectedTo.Includes(dir))
                    return;

                var neighborTile = tile.Neighbour(dir);
                if (logicalTilemap.IsValidTile(neighborTile) && logicalTilemap[neighborTile] is { } neighbor)
                {
                    connections.Add(new Connection(nodes[tile], nodes[neighborTile]));
                }
            }
        }

        private Tilemap<LogicalNode?> optimize(Tilemap<LogicalNode?> logicalTilemap, Random random)
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

        private static readonly FitnessFunction<Tilemap<LogicalNode?>> fitnessFunction = FitnessFunction.From(
            LogicalTilemapFitness.ConnectedComponentsCount,
            LogicalTilemapFitness.ConnectedTrianglesCount,
            LogicalTilemapFitness.NodeBehaviorFitness,
            LogicalTilemapFitness.ConnectionDegreeHistogramDifference(
                new[] {0, /*1*/ 0.15, /*2*/ 0.2, /*3*/ 0.45, /*4*/ 0.2, 0, 0})
        );

        enum MutationType
        {
            ToggleConnection = 1,
            SwitchNodeBlueprint = 2
        }

        private static readonly ImmutableArray<MutationType> mutationTypes = Enum.GetValues<MutationType>().ToImmutableArray();

        private Tilemap<LogicalNode?> mutate(Tilemap<LogicalNode?> currentBest, int mutations, Random random)
        {
            var copy = currentBest.Clone();

            foreach (var _ in Enumerable.Range(0, mutations))
            {
                var mutation = mutationTypes.RandomElement(random);

                var tile = copy.RandomElement(random);
                if (copy[tile] == null)
                    continue;
                var direction = Tiles.Extensions.Directions.RandomElement(random);
                var neighborTile = tile.Neighbour(direction);
                if (!copy.IsValidTile(neighborTile) || copy[neighborTile] == null)
                    continue;

                switch (mutation)
                {
                    case MutationType.ToggleConnection:

                        copy[tile] = copy[tile].WithInverseConnection(direction);
                        copy[neighborTile] = copy[neighborTile].WithInverseConnection(direction.Opposite());

                        break;
                    case MutationType.SwitchNodeBlueprint:

                        var tileBlueprint = copy[tile].Blueprint;
                        copy[tile] = copy[tile] with {Blueprint = copy[neighborTile].Blueprint};
                        copy[neighborTile] = copy[neighborTile] with {Blueprint = tileBlueprint};

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return copy;
        }

        private void relax(List<Node> nodes, List<Connection> connections, Unit radius)
        {
            var allPairs =
                (from n1 in nodes
                    from n2 in nodes.TakeWhile(n => n != n1)
                    select (n1, n2)).ToList();

            foreach (var _ in Enumerable.Range(0, 100))
            {
                foreach (var connection in connections)
                {
                    var diff = connection.Node1.Position - connection.Node2.Position;
                    var dSquared = diff.LengthSquared;

                    var maxD = connection.Node1.Radius + connection.Node2.Radius;
                    var maxDSquared = maxD.Squared;

                    if (maxDSquared > dSquared)
                        continue;

                    var forceMagnitude = (dSquared.NumericValue - maxDSquared.NumericValue).U() * 0.01f;

                    var forceOnN1 = diff / dSquared.Sqrt() * -forceMagnitude;

                    connection.Node1.Position += forceOnN1;
                    connection.Node2.Position -= forceOnN1;
                }

                foreach (var (n1, n2) in allPairs)
                {
                    var diff = n1.Position - n2.Position;
                    var dSquared = diff.LengthSquared;

                    var minD = n1.Radius + n2.Radius;
                    var minDSquared = minD.Squared;

                    if (minDSquared < dSquared)
                        continue;

                    var forceMagnitude = (minDSquared.NumericValue - dSquared.NumericValue).U() * 0.01f;

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
                            (lineDistance.NumericValue.Squared() - projection.NumericValue.Squared()).U() * 0.01f;

                        node.Position += lineNormal * forceMagnitude;
                    }
                }
            }
        }
    }
}
