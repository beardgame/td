using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Debug.LevelDebugMetadata;
using Tile = Bearded.TD.Tiles.Tile;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    record Connection(Node Node1, Node Node2);

    sealed class PhysicalTilemapGenerator
    {
        private readonly Logger logger;
        private readonly LevelDebugMetadata metadata;

        public PhysicalTilemapGenerator(Logger logger, LevelDebugMetadata metadata)
        {
            this.logger = logger;
            this.metadata = metadata;
        }

        private sealed record CreviceData(
            List<TileEdge> Crevices,
            List<RelaxationCircle> Circles,
            MultiDictionary<TileEdge, RelaxationCircle> CircleLookup,
            Dictionary<RelaxationCircle, TileEdge> CreviceByCircle,
            List<Spring> Springs
            );

        sealed record FeatureWithTiles(ImmutableHashSet<Tile> Tiles, Feature Feature)
        {
            public void GenerateTiles(Tilemap<TileGeometry> tilemap) => Feature.GenerateTiles(tilemap, Tiles);
        }

        private abstract record Feature
        {
            public abstract void GenerateTiles(Tilemap<TileGeometry> tilemap, ImmutableHashSet<Tile> tiles);

            public FeatureWithTiles WithTiles(IEnumerable<Tile> tiles) =>
                new FeatureWithTiles(tiles.ToImmutableHashSet(), this);
        }

        sealed record NodeFeature(Node Node) : Feature
        {
            public override void GenerateTiles(Tilemap<TileGeometry> tilemap, ImmutableHashSet<Tile> tiles)
            {
                var context = new NodeGenerationContext(tilemap, tiles);
                foreach (var behavior in Node.Logical.Blueprint!.Behaviors)
                {
                    behavior.Generate(context);
                }
            }
        }

        sealed record CreviceFeature : Feature
        {
            public override void GenerateTiles(Tilemap<TileGeometry> tilemap, ImmutableHashSet<Tile> tiles)
            {
                foreach (var tile in tiles)
                {
                    tilemap[tile] = new TileGeometry(TileType.Crevice, 1, -5.U());
                }
            }
        }

        sealed record ConnectionFeature : Feature
        {
            public override void GenerateTiles(Tilemap<TileGeometry> tilemap, ImmutableHashSet<Tile> tiles)
            {
                foreach (var tile in tiles)
                {
                    tilemap[tile] = new TileGeometry(TileType.Floor, 1, Unit.Zero);
                }
            }
        }

        public Tilemap<TileGeometry> Generate(LogicalTilemap logicalTilemap, Random random, int radius, Unit nodeRadius)
        {
            // TODO: already create Features here and pass them into assignTilesToFeatures below
            // might need another FeatureWithArea type
            // - area could be either circles or a line segment
            // - though we do assign tiles differently based on feature type, how to handle that?
            var nodeDictionary = createRelaxationCircleForLogicalTiles(logicalTilemap, random, nodeRadius);
            var nodes = nodeDictionary.Values.ToList();
            var connections = createNodeConnections(logicalTilemap, nodeDictionary);
            var crevices = createCreviceData(logicalTilemap, nodeRadius);
            var allSprings = createSprings(connections, nodes, crevices);
            var allCircles = nodes.Concat(crevices.Circles).ToList();

            var springSystem = new SpringSystem(allCircles, allSprings, radius.U());

            springSystem.Relax();

            var features = assignTilesToFeatures(radius, nodes, crevices, connections);

            var finalTilemap = generateFinalTilemap(radius, features);

            addNodeMetadata(allCircles);
            addConnectionMetadata(connections);
            addFeaturesMetadata(features);

            return finalTilemap;
        }

        private List<FeatureWithTiles> assignTilesToFeatures(int radius, List<Node> nodes, CreviceData crevices, List<Connection> connections)
        {
            var nodeAreas = getNodeAreas(radius, nodes);
            var tilesOfAllNodes = nodeAreas.Values.SelectMany(t => t).ToHashSet();
            var creviceAreas = getCreviceAreas(radius, crevices, tilesOfAllNodes);
            var connectionAreas = getConnectionAreas(connections, tilesOfAllNodes);

            return enumerateFeatures().ToList();

            IEnumerable<FeatureWithTiles> enumerateFeatures()
            {
                foreach (var (node, tiles) in nodeAreas)
                    yield return new NodeFeature(node).WithTiles(tiles);

                foreach (var (_, tiles) in creviceAreas)
                    yield return new CreviceFeature().WithTiles(tiles);

                foreach (var (_, tiles) in connectionAreas)
                    yield return new ConnectionFeature().WithTiles(tiles);
            }
        }

        private static Tilemap<TileGeometry> generateFinalTilemap(int radius, IEnumerable<FeatureWithTiles> features)
        {
            var tilemap = new Tilemap<TileGeometry>(radius, _ => new TileGeometry(TileType.Wall, 1, Unit.Zero));

            foreach (var feature in features)
            {
                feature.GenerateTiles(tilemap);
            }

            return tilemap;
        }

        private static Dictionary<Connection, HashSet<Tile>> getConnectionAreas(List<Connection> connections, HashSet<Tile> nodeAreas)
        {
            var connectionAreas = new Dictionary<Connection, HashSet<Tile>>();

            foreach (var connection in connections)
            {
                var rayCaster = new LevelRayCaster();
                rayCaster.StartEnumeratingTiles(new Ray(connection.Node1.Position,
                    connection.Node2.Position - connection.Node1.Position));

                var connectionArea = new HashSet<Tile>();
                var adding = false;

                foreach (var tile in rayCaster)
                {
                    if (nodeAreas.Contains(tile))
                    {
                        if (adding)
                            break;
                        continue;
                    }

                    adding = true;

                    connectionArea.Add(tile);
                }

                connectionAreas.Add(connection, connectionArea);
            }

            return connectionAreas;
        }

        private static Dictionary<TileEdge, HashSet<Tile>> getCreviceAreas(int radius, CreviceData crevices,
            HashSet<Tile> reservedNodes)
        {
            var creviceAreas = crevices.Crevices.ToDictionary(c => c, _ => new HashSet<Tile>());
            foreach (var tile in Tilemap.EnumerateTilemapWith(radius).Except(reservedNodes))
            {
                var tilePosition = Level.GetPosition(tile);

                var node = crevices.Circles.MinBy(n => ((tilePosition - n.Position).Length - n.Radius).NumericValue);

                var distanceToNodeSquared = (tilePosition - node.Position).LengthSquared;

                if (distanceToNodeSquared > node.Radius.Squared)
                    continue;

                var crevice = crevices.CreviceByCircle[node];

                creviceAreas[crevice].Add(tile);
            }

            return creviceAreas;
        }

        private Dictionary<Node, HashSet<Tile>> getNodeAreas(int radius, List<Node> nodes)
        {
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

            return nodeAreas;
        }

        private void addFeaturesMetadata(IEnumerable<FeatureWithTiles> features)
        {
            foreach (var feature in features)
            {
                var color = feature.Feature switch
                {
                    ConnectionFeature =>  Color.Beige,
                    CreviceFeature => Color.Brown,
                    NodeFeature => Color.IndianRed,
                    _ => throw new ArgumentOutOfRangeException(nameof(feature))
                };

                metadata.Add(new AreaBorder(TileAreaBorder.From(feature.Tiles), color * 0.5f));
            }
        }

        private void addConnectionMetadata(List<Connection> connections)
        {
            foreach (var connection in connections)
            {
                metadata.Add(new LineSegment(
                    connection.Node1.Position,
                    connection.Node2.Position,
                    Color.Azure * 0.5f
                ));
            }
        }

        private void addNodeMetadata(List<RelaxationCircle> allCircles)
        {
            const float lineHeight = 0.5f;

            foreach (var circle in allCircles)
            {
                metadata.Add(new LevelDebugMetadata.Circle(circle.Position, circle.Radius, 0.3.U(), Color.Cyan * 0.5f));

                if (circle is not Node node)
                    continue;

                foreach (var (behavior, i) in node.Logical.Blueprint.Behaviors.Indexed())
                {
                    metadata.Add(new Text(
                        node.Position - new Difference2(0, i * lineHeight),
                        behavior.Name ?? "",
                        Color.Cyan * 0.5f, 0, lineHeight.U()
                    ));
                }
            }
        }

        private static IEnumerable<Spring> createSprings(List<Connection> connections, List<Node> nodes, CreviceData crevices)
        {
            var connectionSprings = connections.Select(c => new Spring(c.Node1, c.Node2, SpringBehavior.Pull));

            var nodePairAvoidanceSprings =
                from n1 in nodes
                from n2 in nodes.TakeWhile(n => n != n1)
                select new Spring(n1, n2, SpringBehavior.Push);

            var creviceNodeAvoidanceSprings =
                from n in nodes
                from c in crevices.Circles
                select new Spring(n, c, SpringBehavior.Push, Overlap: 1.U());

            var allSprings = connectionSprings
                .Concat(nodePairAvoidanceSprings)
                .Concat(crevices.Springs)
                .Concat(creviceNodeAvoidanceSprings);

            return allSprings;
        }

        private CreviceData createCreviceData(LogicalTilemap logicalTilemap, Unit nodeRadius)
        {
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
                        (Level.GetPosition(tile).NumericValue * 2 + direction.CornerBefore() * toOuterRadius) *
                        nodeRadius;
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

            return new CreviceData(crevices, creviceCircles, creviceCircleLookup, creviceByCircle, creviceSprings);
        }

        private Dictionary<Tile, Node> createRelaxationCircleForLogicalTiles(LogicalTilemap logicalTilemap, Random random, Unit nodeRadius)
        {
            var nodes = new Dictionary<Tile, Node>();
            foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTilemap.Radius))
            {
                var node = logicalTilemap[tile];
                if (node.Blueprint == null)
                    continue;

                var center = Position2.Zero + Level.GetPosition(tile).NumericValue * nodeRadius * 2;

                var n = new Node(center, nodeRadius * random.NextFloat(0.75f, 1.2f), node);
                nodes.Add(tile, n);
            }

            return nodes;
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
    }
}
