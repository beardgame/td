using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.Fitness;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Generation.Semantic.Logical
{
    using Tilemap = LogicalTilemap;
    using FF = SimpleFitnessFunction<LogicalTilemap>;

    static class LogicalTilemapFitness
    {
        public static FF AcuteAnglesCount { get; } = new AcuteAngles();
        public static FF ConnectedComponentsCount { get; } = new ConnectedComponents();
        public static FF DisconnectedCrevices { get; } = new ConnectedCrevices();
        public static FF ConnectedTrianglesCount { get; } = new ConnectedTriangles();

        public static FF NodeBehaviorFitness { get; } = new NodeBehavior();

        private sealed class NodeBehavior : SimpleFitnessFunction<LogicalTilemap>
        {
            public override string Name => "Node Behaviour";

            protected override double CalculateFitness(Tilemap tilemap)
            {
                return (
                    from tile in Tiles.Tilemap.EnumerateTilemapWith(tilemap.Radius)
                    let node = tilemap[tile]
                    where node.Blueprint != null
                    select node.Blueprint!.Behaviors.Sum(b => b.GetFitnessPenalty(tilemap, tile))
                ).Sum();
            }
        }

        public static FF ConnectionDegreeHistogramDifference(IEnumerable<double> idealHistogram) =>
            new ConnectionDegreeHistogram(idealHistogram);

        private sealed class ConnectionDegreeHistogram : FF
        {
            private readonly ImmutableArray<double> targetHistogram;
            public override string Name => "Connection Degree Histogram Difference";

            public ConnectionDegreeHistogram(IEnumerable<double> idealHistogram)
            {
                var builder = ImmutableArray.CreateBuilder<double>(7);
                builder.AddRange(idealHistogram);

                if (builder.Count != 7)
                    throw new ArgumentOutOfRangeException(nameof(idealHistogram), "Ideal histogram must have 7 values");

                var sum = builder.Sum();
                for (var i = 0; i < 7; i++)
                {
                    builder[i] /= sum;
                }

                targetHistogram = builder.MoveToImmutable();
            }

            protected override double CalculateFitness(Tilemap tilemap)
            {
                var actualHistogram = new int[7];
                var tileCount = 0;

                foreach (var tile in Tiles.Tilemap.EnumerateTilemapWith(tilemap.Radius)
                    .Select(t => tilemap[t]).Where(n => n.Blueprint != null))
                {
                    var connections = tile.ConnectedTo.Enumerate().Count();
                    actualHistogram[connections]++;
                    tileCount++;
                }

                var distance = targetHistogram
                    .Zip(actualHistogram, (target, actual) => Math.Abs(target * tileCount - actual))
                    .Sum();

                return distance;
            }
        }

        private sealed class ConnectedTriangles : FF
        {
            public override string Name => "Connected Triangles";

            protected override double CalculateFitness(Tilemap tilemap)
            {
                var count = 0;

                foreach (var (tile, node) in Tiles.Tilemap.EnumerateTilemapWith(tilemap.Radius)
                    .Select(t => (t, tilemap[t])))
                {
                    var connected = node.ConnectedTo;
                    count += Extensions.Directions.Count(direction
                        => connected.Includes(direction)
                        && connected.Includes(direction.Next())
                        && tilemap[tile.Neighbor(direction)]!.ConnectedTo.Includes(direction.Next().Next()));
                }

                return count;
            }
        }

        private sealed class AcuteAngles : FF
        {
            public override string Name => "Acute Angles";

            protected override double CalculateFitness(Tilemap tilemap)
            {
                return Tiles.Tilemap.EnumerateTilemapWith(tilemap.Radius)
                    .Select(t => tilemap[t])
                    .Select(node => node.ConnectedTo)
                    .Sum(connected => Extensions.Directions.Count(direction
                        => connected.Includes(direction)
                        && connected.Includes(direction.Next())));
            }
        }

        private sealed class ConnectedComponents : FF
        {
            public override string Name => "Connected Components";

            protected override double CalculateFitness(Tilemap tilemap)
            {
                var componentCount = 0;
                var seen = new HashSet<Tile>();
                foreach (var tile in Tiles.Tilemap.EnumerateTilemapWith(tilemap.Radius))
                {
                    if (seen.Contains(tile) || tilemap[tile].Blueprint == null)
                        continue;

                    fillFrom(tile);
                    componentCount++;
                }

                return componentCount * 100;

                void fillFrom(Tile tile)
                {
                    if (!seen.Add(tile))
                        return;

                    var node = tilemap[tile];
                    foreach (var direction in node.ConnectedTo.Enumerate())
                    {
                        fillFrom(tile.Neighbor(direction));
                    }
                }
            }
        }

        private sealed class ConnectedCrevices : FF
        {
            public override string Name => "Connected Crevices";
            protected override double CalculateFitness(LogicalTilemap tilemap)
            {
                var corners = enumerateCorners(tilemap);

                var penalty = 0;

                foreach (var corner in corners)
                {
                    var edges = corner.IncidentEdges;

                    var incidentCrevices = edges.Enumerate().Count(e => tilemap[e].Feature is Crevice);

                    penalty += incidentCrevices switch
                    {
                        1 => 2,
                        3 => 1,
                        _ => 0,
                    };
                }

                return penalty;
            }

            private IEnumerable<TileCorner> enumerateCorners(LogicalTilemap tilemap)
            {
                var virtualRadius = tilemap.Radius + 1;

                foreach (var tile in Tiles.Tilemap.EnumerateTilemapWith(virtualRadius))
                {
                    if (tile.Neighbor(Direction.UpRight).Radius > virtualRadius)
                        continue;

                    if (tile.Neighbor(Direction.Right).Radius <= virtualRadius)
                    {
                        yield return TileCorner.FromTileAndDirectionBefore(tile, Direction.Right);
                    }

                    if (tile.Neighbor(Direction.UpLeft).Radius <= virtualRadius)
                    {
                        yield return TileCorner.FromTileAndDirectionBefore(tile, Direction.UpRight);
                    }
                }
            }

        }
    }
}
