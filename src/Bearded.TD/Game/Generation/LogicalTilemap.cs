using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Tiles;
using Google.Protobuf;

namespace Bearded.TD.Game.Generation
{
    sealed record LogicalNode(NodeBlueprint? Blueprint, Directions ConnectedTo,
        ImmutableDictionary<Direction, MacroFeature> MacroFeatures)
    {
    }

    sealed record EdgeFeatures(bool IsConnected, MacroFeature? Feature)
    {
        public static EdgeFeatures Default { get; } = new(false, null);
    }

    record MacroFeature;

    record Crevice : MacroFeature;

    sealed class LogicalTilemap
    {
        record TileNode(NodeBlueprint? Node, EdgeFeatures Right, EdgeFeatures UpRight, EdgeFeatures UpLeft)
        {
            public static TileNode Default { get; } =
                new(null, EdgeFeatures.Default, EdgeFeatures.Default, EdgeFeatures.Default);

            public static TileNode DefaultWith(NodeBlueprint? node) =>
                node == null ? Default : Default with {Node = node};
        }

        private readonly Tilemap<TileNode> tiles;

        public int Radius { get; }

        public static LogicalTilemap From(Tilemap<NodeBlueprint?> nodes,
            IDictionary<(Tile Tile, Direction Direction), MacroFeature> macroFeatures)
        {
            var macroFeaturesLookup =
                macroFeatures.ToDictionary(kvp => normalize(kvp.Key.Tile, kvp.Key.Direction), kvp => kvp.Value);

            var tiles = new Tilemap<TileNode>(nodes.Radius + 1,
                tile =>
                {
                    var node = nodes.IsValidTile(tile) ? nodes[tile] : null;

                    return new TileNode(
                        node,
                        edgeFeatures(tile, Direction.Right),
                        edgeFeatures(tile, Direction.UpRight),
                        edgeFeatures(tile, Direction.UpLeft));
                });

            return new(tiles);

            EdgeFeatures edgeFeatures(Tile tile, Direction direction)
                => new(false, macroFeaturesLookup.GetValueOrDefault((tile, direction)));
        }

        private LogicalTilemap(Tilemap<TileNode> tiles)
        {
            Radius = tiles.Radius - 1;

            this.tiles = tiles;
        }

        public bool IsValidTile(Tile tile) => tile.Radius <= Radius;

        public LogicalNode this[Tile tile]
        {
            get => new(
                tiles[tile].Node,
                Extensions.Directions
                    .Where(d => this[tile, d].IsConnected)
                    .Aggregate(Directions.None, (directions, direction) => directions.And(direction)),
                Extensions.Directions
                    .Where(d => this[tile, d].Feature != null)
                    .ToImmutableDictionary(d => d, d => this[tile, d].Feature));
        }

        public void InvertConnectivity(Tile tile, Direction direction)
        {
            var edge = this[tile, direction];
            var newEdge = edge with {IsConnected = !edge.IsConnected};
            setEdgeFeatures(tile, direction, newEdge);
        }

        public void SwapNodes(Tile tile1, Tile tile2)
        {
            var node1 = tiles[tile1];
            var node2 = tiles[tile2];

            tiles[tile1] = node1 with {Node = node2.Node};
            tiles[tile2] = node2 with {Node = node1.Node};
        }

        public void SwitchMacroFeatures(Tile tile, Direction direction1, Direction direction2)
        {
            var edge1 = this[tile, direction1];
            var edge2 = this[tile, direction2];

            setEdgeFeatures(tile, direction1, edge1 with {Feature = edge2.Feature});
            setEdgeFeatures(tile, direction2, edge2 with {Feature = edge1.Feature});
        }

        private void setEdgeFeatures(Tile tile, Direction direction, EdgeFeatures features)
        {
            (tile, direction) = normalize(tile, direction);
            switch (direction)
            {
                case Direction.Right:
                    tiles[tile] = tiles[tile] with {Right = features};
                    break;
                case Direction.UpRight:
                    tiles[tile] = tiles[tile] with {UpRight = features};
                    break;
                case Direction.UpLeft:
                    tiles[tile] = tiles[tile] with {UpLeft = features};
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public EdgeFeatures this[Tile tile, Direction direction]
        {
            get
            {
                (tile, direction) = normalize(tile, direction);
                return direction switch
                {
                    Direction.Right => tiles[tile].Right,
                    Direction.UpRight => tiles[tile].UpRight,
                    Direction.UpLeft => tiles[tile].UpLeft,
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                };
            }
        }

        private static (Tile Tile, Direction Direction) normalize(Tile tile, Direction direction)
        {
            return direction switch
            {
                Direction.Left or Direction.DownLeft or Direction.DownRight =>
                    (tile.Neighbour(direction), direction.Opposite()),
                _ => (tile, direction)
            };
        }

        public LogicalTilemap Clone() => new(tiles.Clone());
    }
}
