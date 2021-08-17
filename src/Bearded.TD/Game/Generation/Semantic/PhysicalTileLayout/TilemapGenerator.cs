using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed class TilemapGenerator
    {
        public (Tilemap<TileGeometry> Tilemap, ImmutableArray<CommandFactory> Commands) GenerateTilemap(
            int radius, IEnumerable<TiledFeature> features, Random random)
        {
            var tilemap = new Tilemap<TileGeometry>(radius, _ => new TileGeometry(TileType.Wall, 1, Unit.Zero));
            var commandAccumulator = new LevelGenerationCommandAccumulator();

            foreach (var feature in features)
            {
                switch (feature)
                {
                    case TiledFeature.Node node:
                        generateNode(feature, tilemap, node, commandAccumulator, random);
                        break;
                    case (PhysicalFeature.Connection, _):
                        generateConnection(feature, tilemap);
                        break;
                    case (PhysicalFeature.Crevice, _):
                        generateCrevice(feature, tilemap);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            return (tilemap, commandAccumulator.ToCommandFactories());
        }

        private void generateNode(TiledFeature feature, Tilemap<TileGeometry> tilemap, TiledFeature.Node node,
            LevelGenerationCommandAccumulator commandAccumulator, Random random)
        {
            var context = NodeGenerationContext.Create(tilemap,
                feature.Tiles,
                node.NodeFeature.Circles,
                node.Connections,
                commandAccumulator,
                random);
            foreach (var behavior in node.NodeFeature.Blueprint.Behaviors)
            {
                behavior.Generate(context);
            }

            ensureConnectedness(context);
        }

        private void ensureConnectedness(NodeGenerationContext context)
        {
            var connections = context.NodeData.Connections;

            if (connections.Length < 2)
                return;

            var expensivePathMinimum = context.Tiles.All.Count * 2;

            var pathfinder = Pathfinder
                .WithTileCosts(t => context.Tiles.Get(t).Type == TileType.Floor ? 1 : expensivePathMinimum, 1)
                .InArea(context.Tiles.All);

            foreach (var tile in connections.Skip(1))
            {
                var result = pathfinder.FindPath(tile, connections[0]);

                if (result == null)
                    throw new InvalidOperationException();

                var currentTile = tile;

                if (result.Cost < expensivePathMinimum)
                    continue;

                foreach (var step in result.Path)
                {
                    currentTile = currentTile.Neighbor(step);
                    setFloor(currentTile);
                }
            }

            foreach (var tile in connections)
            {
                setFloor(tile);
            }

            void setFloor(Tile tile) => context.Tiles.Set(tile, new TileGeometry(TileType.Floor, 0, Unit.Zero));
        }

        private static void generateCrevice(TiledFeature feature, Tilemap<TileGeometry> tilemap)
        {
            foreach (var tile in feature.Tiles)
            {
                tilemap[tile] = new TileGeometry(TileType.Crevice, 1, -5.U());
            }
        }

        private static void generateConnection(TiledFeature feature, Tilemap<TileGeometry> tilemap)
        {
            foreach (var tile in feature.Tiles)
            {
                tilemap[tile] = new TileGeometry(TileType.Floor, 1, Unit.Zero);
            }
        }
    }
}
