using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Debug;
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

            public Node(Position2 position, Unit radius)
            {
                Position = position;
                Radius = radius;
            }
        }

        public Tilemap<TileGeometry> Generate(int radius, int seed)
        {
            logger.Debug?.Log($"Started generating map with radius {radius} and seed {seed}");
            var timer = Stopwatch.StartNew();

            var random = new Random(seed);

            var area = Tilemap.TileCountForRadius(radius);
            var areaPerNode = 12 * 12;
            var nodeCount = area / areaPerNode * 2 / 3;
            var nodeRadius = ((float) areaPerNode).Sqrted().U() * 0.5f;

            var unitRadius = radius * HexagonWidth.U();

            var nodes = Enumerable.Range(0, nodeCount).Select(_ =>
            {
                var dir = Direction2.FromDegrees(random.NextFloat(360));
                var d = random.NextFloat().Sqrted() * unitRadius;
                var p = Position2.Zero + dir * d;

                var r = random.NormalFloat(1, 0.2f).Clamped(0.75f, 1.2f) * nodeRadius;
                return new Node(p, r);
            }).ToList();

            relax(nodes, radius.U());

            foreach (var node in nodes)
            {
                metadata.Add(new Circle(node.Position, node.Radius, 0.3.U(), Color.Cyan * 0.5f));
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

            logger.Debug?.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

            return new Tilemap<TileGeometry>(radius, _ => new TileGeometry(TileType.Floor, 1, Unit.Zero));
        }

        private void relax(List<Node> nodes, Unit radius)
        {
            var allPairs =
                (from n1 in nodes
                    from n2 in nodes.TakeWhile(n => n != n1)
                    where n1 != n2
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

                    var forceMagnitude = (maxDSquared.NumericValue.U() - dSquared.NumericValue.U()) * 0.01f;

                    var forceOnN1 = diff / dSquared.Sqrt() * forceMagnitude;

                    n1.Position += forceOnN1;
                    n2.Position -= forceOnN1;
                }

                foreach (var node in nodes)
                {
                    foreach (var direction in Tiles.Extensions.Directions)
                    {
                        var p0 = direction.Vector() * radius;
                        var p1 = direction.Next().Vector() * radius;

                        var lineNormal = direction.CornerAfter();
                        var lineDistance = HexagonDistanceY * radius - node.Radius;

                        var projection = Vector2.Dot(lineNormal, node.Position.NumericValue).U();

                        if (projection < lineDistance)
                            continue;

                        var forceMagnitude = (lineDistance.NumericValue.Squared() - projection.NumericValue.Squared()).U() * 0.1f;

                        node.Position += lineNormal * forceMagnitude;
                    }
                }
            }
        }
    }
}
