using System;
using System.Linq;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation
{
    record LogicalNode(NodeBlueprint? Blueprint, Directions ConnectedTo)
    {
    }

    record EdgeFeatures(bool IsConnected, MacroFeature? Feature)
    {
        public static EdgeFeatures Default { get; } = new(false, null);
    }

    record MacroFeature;

    class LogicalTilemap
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

        public static LogicalTilemap From(Tilemap<NodeBlueprint?> nodes)
        {
            var tiles = new Tilemap<TileNode>(nodes.Radius + 1, tile =>
                nodes.IsValidTile(tile) ? TileNode.DefaultWith(nodes[tile]) : TileNode.Default);

            return new(tiles);
        }

        private LogicalTilemap(Tilemap<TileNode> tiles)
        {
            Radius = tiles.Radius - 1;

            this.tiles = tiles;
        }

        public bool IsValidTile(Tile tile) => tile.Radius <= Radius;

        public LogicalNode this[Tile tile]
        {
            get
            {
                return new LogicalNode(
                    tiles[tile].Node, Extensions.Directions
                        .Where(d => this[tile, d].IsConnected)
                        .Aggregate(Directions.None, (directions, direction) => directions.And(direction))
                );
            }
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

        private void setEdgeFeatures(Tile tile, Direction direction, EdgeFeatures features)
        {
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
                case Direction.Left:
                case Direction.DownLeft:
                case Direction.DownRight:
                    setEdgeFeatures(tile.Neighbour(direction), direction.Opposite(), features);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public EdgeFeatures this[Tile tile, Direction direction]
        {
            get
            {
                return direction switch
                {
                    Direction.Right => tiles[tile].Right,
                    Direction.UpRight => tiles[tile].UpRight,
                    Direction.UpLeft => tiles[tile].UpLeft,
                    Direction.Left or Direction.DownLeft or Direction.DownRight =>
                        this[tile.Neighbour(direction), direction.Opposite()],
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                };
            }
        }

        public LogicalTilemap Clone() => new(tiles.Clone());
    }
}
