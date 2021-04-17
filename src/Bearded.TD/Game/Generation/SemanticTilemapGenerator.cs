using System;
using System.Collections;
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
        double GetFitnessPenalty(LogicalTilemap tilemap, Tile nodeTile) => 0;

        IEnumerable<NodeTag> Tags => Enumerable.Empty<NodeTag>();

        void Generate(NodeGenerationContext context)
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
        public double GetFitnessPenalty(LogicalTilemap tilemap, Tile nodeTile)
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

        public double GetFitnessPenalty(LogicalTilemap tilemap, Tile nodeTile)
        {
            var node = tilemap[nodeTile];

            var connectedNodes = Extensions.Directions
                .Where(d => node!.ConnectedTo.Includes(d))
                .Select(nodeTile.Neighbour)
                .Select(t => tilemap[t]);

            return connectedNodes.Count(n => n.Blueprint!.Tags.Contains(tagToAvoid)) * 100;
        }
    }

    class ClearAllTiles : INodeBehavior
    {
        public void Generate(NodeGenerationContext context)
        {
            foreach (var t in context.Tiles)
            {
                context.Set(t, new TileGeometry(TileType.Floor, 0, 0.U()));
            }
        }
    }

    class NodeGenerationContext
    {
        private readonly Tilemap<TileGeometry> tilemap;

        public ImmutableHashSet<Tile> Tiles { get; }

        // info about connections, etc.

        public NodeGenerationContext(Tilemap<TileGeometry> tilemap, IEnumerable<Tile> tiles)
        {
            this.tilemap = tilemap;
            Tiles = tiles.ToImmutableHashSet();
        }

        public void Set(Tile tile, TileGeometry geometry)
        {
            if (!Tiles.Contains(tile))
                throw new ArgumentException("May not write to tile outside node.", nameof(tile));

            tilemap[tile] = geometry;
        }
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

        class RelaxationCircle
        {
            public Position2 Position { get; set; }
            public Unit Radius { get; }

            public RelaxationCircle(Position2 position, Unit radius)
            {
                Position = position;
                Radius = radius;
            }
        }

        enum SpringBehavior
        {
            Push,
            Pull,
        }

        record Spring(RelaxationCircle Circle1, RelaxationCircle Circle2, SpringBehavior Behavior,
            float ForceMultiplier = 1, Unit Overlap = default)
        {
            public Unit TargetDistance => Circle1.Radius + Circle2.Radius - Overlap;
        }

        class Node : RelaxationCircle
        {
            public LogicalNode Logical { get; }

            public Node(Position2 position, Unit radius, LogicalNode logical)
                : base(position, radius)
            {
                Logical = logical;
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
                .ToDictionary(t => t.Edge(normalizedDirections.RandomElement(random)), _ => new Crevice() as MacroFeature);

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

            foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTilemap.Radius))
            {
                var node = logicalTilemap[tile];

                var center = Position2.Zero + Level.GetPosition(tile).NumericValue * nodeRadius * 2;

                const float toOuterRadius = 2 / 1.73205080757f;
                foreach (var (direction, feature) in node.MacroFeatures)
                {
                    var before = center + direction.CornerBefore() * nodeRadius * toOuterRadius;
                    var after = center + direction.CornerAfter() * nodeRadius * toOuterRadius;

                    metadata.Add(new LineSegment(before, after, Color.Beige * 0.1f));
                }

                if (node.Blueprint == null)
                    continue;
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
            foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTilemap.Radius))
            {
                var node = logicalTilemap[tile];
                if (node.Blueprint == null)
                    continue;

                var center = Position2.Zero + Level.GetPosition(tile).NumericValue * nodeRadius * 2;

                var n = new Node(center, nodeRadius * random.NextFloat(0.75f, 1.2f), node);
                nodes.Add(n);
                nodeDictionary.Add(tile, n);
            }

            var connections = createNodeConnections(logicalTilemap, nodeDictionary);

            var connectionSprings = connections.Select(c => new Spring(c.Node1, c.Node2, SpringBehavior.Pull));
            var nodePairAvoidanceSprings =
                from n1 in nodes
                from n2 in nodes.TakeWhile(n => n != n1)
                select new Spring(n1, n2, SpringBehavior.Push);

            var crevices = new List<TileEdge>();
            var creviceCircles = new List<RelaxationCircle>();
            var creviceCircleLookup = new MultiDictionary<TileEdge, RelaxationCircle>();
            var creviceByCircle = new Dictionary<RelaxationCircle, TileEdge>();
            var creviceSprings = new List<Spring>();
            foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTilemap.Radius))
            {
                var node = logicalTilemap[tile];
                foreach (var (direction, feature) in node.MacroFeatures)
                {
                    if (feature is not Crevice crevice)
                        continue;
                    var edge = tile.Edge(direction);
                    if (creviceCircleLookup.ContainsKey(edge))
                        continue;

                    crevices.Add(edge);

                    const float toOuterRadius = 2 / 1.73205080757f;
                    var p1 = Position2.Zero +
                        (Level.GetPosition(tile).NumericValue * 2 + direction.CornerBefore() * toOuterRadius) * nodeRadius;
                    var p1to2 = (direction.CornerAfter() - direction.CornerBefore()) * toOuterRadius * nodeRadius;


                    var count = 5;

                    var circles = Enumerable.Range(0, count).Select(i =>
                    {
                        var p = p1 + p1to2 * (i / (float) (count - 1));
                        return new RelaxationCircle(p, 1.5.U());
                    }).ToList();

                    creviceCircles.AddRange(circles);
                    circles.ForEach(c => creviceCircleLookup.Add(edge, c));
                    circles.ForEach(c => creviceByCircle.Add(c, edge));
                    creviceSprings.AddRange(Enumerable.Range(0, count - 1).Select(
                        i => new Spring(circles[i], circles[i + 1], SpringBehavior.Pull, 5, 0.5.U())
                        ));
                }
            }

            var creviceNodeAvoidanceSprings =
                from n in nodes
                from c in creviceCircles
                select new Spring(n, c, SpringBehavior.Push, Overlap:1.U());

            var allSprings = connectionSprings
                .Concat(nodePairAvoidanceSprings)
                .Concat(creviceSprings)
                .Concat(creviceNodeAvoidanceSprings);
            var allCircles = nodes.Concat(creviceCircles).ToList();

            relax(allCircles, allSprings, radius.U());

            const float metaLineHeight = 0.5f;

            foreach (var circle in allCircles)
            {
                metadata.Add(new LevelDebugMetadata.Circle(circle.Position, circle.Radius, 0.3.U(), Color.Cyan * 0.5f));

                if (circle is not Node node)
                    continue;

                foreach (var (behavior, i) in node.Logical.Blueprint.Behaviors.Indexed())
                {
                    metadata.Add(new Text(
                        node.Position - new Difference2(0, i * metaLineHeight),
                        behavior.Name ?? "",
                        Color.Cyan * 0.5f, 0, metaLineHeight.U()
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

            var creviceAreas = crevices.ToDictionary(c => c, _ => new HashSet<Tile>());
            foreach (var tile in Tilemap.EnumerateTilemapWith(radius))
            {
                var tilePosition = Level.GetPosition(tile);

                var node = creviceCircles.MinBy(n => ((tilePosition - n.Position).Length - n.Radius).NumericValue);

                var distanceToNodeSquared = (tilePosition - node.Position).LengthSquared;

                if (distanceToNodeSquared > node.Radius.Squared)
                    continue;

                var crevice = creviceByCircle[node];

                creviceAreas[crevice].Add(tile);
            }

            foreach (var (crevice, creviceArea) in creviceAreas)
            {
                foreach (var (node, nodeArea) in nodeAreas)
                {
                    foreach (var tile in creviceArea)
                    {
                        nodeArea.Remove(tile);
                    }
                }
            }

            var connectionAreas = new Dictionary<Connection, HashSet<Tile>>();

            foreach (var connection in connections)
            {
                var areaFrom = nodeAreas[connection.Node1];
                var areaTo = nodeAreas[connection.Node2];

                var rayCaster = new LevelRayCaster();
                rayCaster.StartEnumeratingTiles(new Ray(connection.Node1.Position,
                    connection.Node2.Position - connection.Node1.Position));

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

            foreach (var (_, tiles) in creviceAreas)
            {
                metadata.Add(new AreaBorder(TileAreaBorder.From(tiles), Color.Brown * 0.5f));
            }

            foreach (var (_, tiles) in connectionAreas)
            {
                metadata.Add(new AreaBorder(TileAreaBorder.From(tiles), Color.IndianRed * 0.5f));
            }


            logger.Debug?.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

            var finalTilemap = new Tilemap<TileGeometry>(radius, _ => new TileGeometry(TileType.Wall, 1, Unit.Zero));

            foreach (var (node, tiles) in nodeAreas)
            {
                var context = new NodeGenerationContext(finalTilemap, tiles);
                foreach (var behavior in node.Logical.Blueprint.Behaviors)
                {
                    behavior.Generate(context);
                }
            }

            foreach (var tile in creviceAreas.Values.SelectMany(x => x))
            {
                finalTilemap[tile] = new TileGeometry(TileType.Crevice, 1, -5.U());
            }

            foreach (var tile in connectionAreas.Values.SelectMany(x => x))
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

        private static List<Connection> createNodeConnections(LogicalTilemap logicalTilemap,
            Dictionary<Tile, Node> nodes)
        {
            var connections = new List<Connection>();
            foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTilemap.Radius))
            {
                var node = logicalTilemap[tile];
                if (node.Blueprint == null)
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
                var mutation = mutationTypes.RandomElement(random);

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

        private bool tryCallOnCollectedTilesWithBlueprint(Random random, LogicalTilemap tilemap, Action<TileEdge> action)
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

        private void relax(IEnumerable<RelaxationCircle> circles, IEnumerable<Spring> springs, Unit radius)
        {
            var nodes = circles.ToList();
            var connections = springs.ToList();

            foreach (var _ in Enumerable.Range(0, 100))
            {
                foreach (var connection in connections)
                {
                    var (n1, n2) = (connection.Circle1, connection.Circle2);

                    var diff = n1.Position - n2.Position;
                    var dSquared = diff.LengthSquared;

                    var targetD = connection.TargetDistance;
                    var targetDSquared = targetD.Squared;

                    switch (connection.Behavior)
                    {
                        case SpringBehavior.Push:
                            if (targetDSquared < dSquared)
                                continue;
                            break;
                        case SpringBehavior.Pull:
                            if (targetDSquared > dSquared)
                                continue;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    var forceMagnitude = (targetDSquared.NumericValue - dSquared.NumericValue).U() * 0.01f *
                            connection.ForceMultiplier;
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
