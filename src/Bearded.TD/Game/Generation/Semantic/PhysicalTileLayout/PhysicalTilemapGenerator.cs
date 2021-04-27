using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.Logical;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.World;
using static Bearded.TD.Game.Debug.LevelDebugMetadata;
using Tile = Bearded.TD.Tiles.Tile;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
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

    sealed class PhysicalTilemapGenerator
    {
        private readonly Logger logger;
        private readonly LevelDebugMetadata metadata;

        public PhysicalTilemapGenerator(Logger logger, LevelDebugMetadata metadata)
        {
            this.logger = logger;
            this.metadata = metadata;
        }

        public Tilemap<TileGeometry> Generate(LogicalTilemap logicalTilemap, Random random, int radius, Unit nodeRadius)
        {


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

            var creviceNodeAvoidanceSprings =
                from n in nodes
                from c in creviceCircles
                select new Spring(n, c, SpringBehavior.Push, Overlap: 1.U());

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
