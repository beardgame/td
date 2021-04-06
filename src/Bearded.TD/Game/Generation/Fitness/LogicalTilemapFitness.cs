using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Generation.Fitness
{
    using Tilemap = Tilemap<LogicalNode?>;
    using FF = FitnessFunction<Tilemap<LogicalNode?>>;

    static class LogicalTilemapFitness
    {
        public static FF AcuteAnglesCount { get; } = new AcuteAngles();
        public static FF ConnectedComponentsCount { get; } = new ConnectedComponents();
        public static FF ConnectedTrianglesCount { get; } = new ConnectedTriangles();

        public static FF NodeBehaviorFitness { get; } = new NodeBehavior();

        private class NodeBehavior : FF
        {
            public override string Name => "Node Behaviour";

            protected override double CalculateFitness(Tilemap tilemap)
            {
                return (
                    from tile in tilemap
                    let node = tilemap[tile]
                    where node != null
                    select node.Blueprint.Behaviors.Sum(b => b.GetFitnessPenalty(tilemap, tile))
                ).Sum();
            }
        }

        public static FF ConnectionDegreeHistogramDifference(IEnumerable<double> idealHistogram) =>
            new ConnectionDegreeHistogram(idealHistogram);

        private class ConnectionDegreeHistogram : FF
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

                foreach (var tile in tilemap.Select(t => tilemap[t]).NotNull())
                {
                    var connections = tile.ConnectedTo.Enumerate().Count();
                    actualHistogram[connections]++;
                    tileCount++;
                }

                var distance = Enumerable
                    .Zip(targetHistogram, actualHistogram, (target, actual) => Math.Abs(target * tileCount - actual))
                    .Sum();

                return distance;
            }
        }

        private class ConnectedTriangles : FF
        {
            public override string Name => "Connected Triangles";

            protected override double CalculateFitness(Tilemap tilemap)
            {
                var count = 0;

                foreach (var (tile, node) in tilemap.Select(t => (t, tilemap[t])).Where(t => t.Item2 != null))
                {
                    var connected = node.ConnectedTo;
                    count += Tiles.Extensions.Directions.Count(direction
                        => connected.Includes(direction)
                        && connected.Includes(direction.Next())
                        && tilemap[tile.Neighbour(direction)]!.ConnectedTo.Includes(direction.Next().Next()));
                }

                return count;
            }
        }

        private class AcuteAngles : FF
        {
            public override string Name => "Acute Angles";

            protected override double CalculateFitness(Tilemap tilemap)
            {
                return tilemap
                    .Select(t => tilemap[t]).NotNull()
                    .Select(node => node.ConnectedTo)
                    .Sum(connected => Tiles.Extensions.Directions.Count(direction
                        => connected.Includes(direction)
                        && connected.Includes(direction.Next())));
            }
        }

        private class ConnectedComponents : FF
        {
            public override string Name => "Connected Components";

            protected override double CalculateFitness(Tilemap tilemap)
            {
                var componentCount = 0;
                var seen = new HashSet<Tile>();
                foreach (var tile in tilemap)
                {
                    if (seen.Contains(tile) || tilemap[tile] == null)
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
                    foreach (var direction in node!.ConnectedTo.Enumerate())
                    {
                        fillFrom(tile.Neighbour(direction));
                    }
                }
            }
        }
    }
}
