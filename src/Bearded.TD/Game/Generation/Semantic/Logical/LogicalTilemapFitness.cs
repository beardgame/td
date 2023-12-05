using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.Fitness;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Generation.Semantic.Logical;

using FF = SimpleFitnessFunction<LogicalTilemap>;

static class LogicalTilemapFitness
{
    public static FF ConnectedComponentsCount { get; } = new ConnectedComponents();
    public static FF DisconnectedCrevices { get; } = new ConnectedCrevices();
    public static FF ConnectedTrianglesCount { get; } = new ConnectedTriangles();
    public static FF CriticalConnectionCount { get; } = new CriticalConnections();
    public static FF BiomeComponentCount { get; } = new DisconnectedBiomes();

    public static FF NodeBehaviorFitness { get; } = new NodeBehavior();

    private sealed class NodeBehavior : SimpleFitnessFunction<LogicalTilemap>
    {
        public override string Name => "Node Behaviour";

        protected override double CalculateFitness(LogicalTilemap tilemap)
        {
            return (
                from tile in Tilemap.EnumerateTilemapWith(tilemap.Radius)
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

        protected override double CalculateFitness(LogicalTilemap tilemap)
        {
            var actualHistogram = new int[7];
            var tileCount = 0;

            foreach (var tile in Tilemap.EnumerateTilemapWith(tilemap.Radius)
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

        protected override double CalculateFitness(LogicalTilemap tilemap)
        {
            var count = 0;

            foreach (var (tile, node) in Tilemap.EnumerateTilemapWith(tilemap.Radius)
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

    private sealed class CriticalConnections : FF
    {
        private static readonly ImmutableHashSet<Direction> rightFacingDirections =
            ImmutableHashSet.Create(Direction.Right, Direction.UpRight, Direction.DownRight);

        public override string Name => "Critical Connections";

        protected override double CalculateFitness(LogicalTilemap tilemap)
        {
            var edges = Tilemap.EnumerateTilemapWith(tilemap.Radius)
                .SelectMany(
                    t => tilemap[t].ConnectedTo.Enumerate()
                        .Where(rightFacingDirections.Contains)
                        .Select(dir => t.Edge(dir)));

            var penalty = 0;

            foreach (var (from, to) in edges.Select(e => e.AdjacentTiles))
            {
                if (isConnectedIndirectly(from, to, out var tilesConnectedToFrom))
                {
                    continue;
                }

                penalty += 3;

                if (!anyTagsPresent(tilesConnectedToFrom) ||
                    (!isConnectedIndirectly(to, from, out var tilesConnectedToTo)! &&
                        anyTagsPresent(tilesConnectedToTo)))
                {
                    penalty += 15;
                }
            }

            return penalty;

            bool isConnectedIndirectly(
                Tile start, Tile target, [NotNullWhen(false)] out ImmutableHashSet<Tile>? connectedTiles)
            {
                var q = new Queue<Tile>();
                var seen = new HashSet<Tile>();

                q.Enqueue(start);
                seen.Add(start);

                while (q.TryDequeue(out var currTile))
                {
                    var neighborTiles = tilemap[currTile].ConnectedTo.Enumerate().Select(currTile.Neighbor);
                    foreach (var neighborTile in neighborTiles)
                    {
                        if (neighborTile == target)
                        {
                            // Don't explore tiles across from our direct connection.
                            if (currTile == start)
                            {
                                continue;
                            }

                            connectedTiles = default;
                            return true;
                        }

                        if (seen.Contains(neighborTile))
                        {
                            continue;
                        }

                        q.Enqueue(neighborTile);
                        seen.Add(neighborTile);
                    }
                }

                connectedTiles = ImmutableHashSet.CreateRange(seen);
                return false;
            }

            bool anyTagsPresent(IEnumerable<Tile> tiles) =>
                tiles.SelectMany(t => tilemap[t].Blueprint?.AllTags ?? Enumerable.Empty<NodeTag>()).Any();
        }
    }

    private sealed class ConnectedComponents : FF
    {
        public override string Name => "Connected Components";

        protected override double CalculateFitness(LogicalTilemap tilemap)
        {
            var componentCount = 0;
            var seen = new HashSet<Tile>();
            foreach (var tile in Tilemap.EnumerateTilemapWith(tilemap.Radius))
            {
                if (seen.Contains(tile) || tilemap[tile].Blueprint == null)
                    continue;

                fillFrom(tile);
                componentCount++;
            }

            return componentCount * 500;

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

            foreach (var tile in Tilemap.EnumerateTilemapWith(virtualRadius))
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

    private sealed class DisconnectedBiomes : FF
    {
        public override string Name => "Disconnected biomes";

        protected override double CalculateFitness(LogicalTilemap tilemap)
        {
            var componentCounts = componentCountsByBiome(tilemap);
            var totalExcessComponents = componentCounts.Sum(kvp => kvp.Value - 1);

            return totalExcessComponents * 50;
        }

        private static ImmutableDictionary<IBiome, int> componentCountsByBiome(LogicalTilemap tilemap)
        {
            var result = ImmutableDictionary.CreateBuilder<IBiome, int>();
            var seen = new HashSet<Tile>();

            foreach (var tile in Tilemap.EnumerateTilemapWith(tilemap.Radius))
            {
                if (seen.Contains(tile) || tilemap[tile].Biome == null)
                {
                    continue;
                }

                var biome = tilemap[tile].Biome;
                fillFrom(tile, biome);
                result[biome] = result.GetValueOrDefault(biome) + 1;
            }

            return result.ToImmutable();

            void fillFrom(Tile tile, IBiome biome)
            {
                if (tilemap[tile].Biome != biome || !seen.Add(tile))
                {
                    return;
                }

                var node = tilemap[tile];
                foreach (var direction in node.ConnectedTo.Enumerate())
                {
                    fillFrom(tile.Neighbor(direction), biome);
                }
            }
        }
    }
}
