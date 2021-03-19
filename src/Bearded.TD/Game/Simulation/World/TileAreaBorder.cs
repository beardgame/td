using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.World
{
    class TileAreaBorder
    {
        public readonly struct Part
        {
            public Tile Tile { get; }
            public Direction Direction { get; }
            public bool BeforeIsConvex { get; }
            public bool AfterIsConvex { get; }

            public Part(Tile tile, Direction direction, bool beforeIsConvex, bool afterIsConvex)
            {
                Tile = tile;
                Direction = direction;
                BeforeIsConvex = beforeIsConvex;
                AfterIsConvex = afterIsConvex;
            }

            public void Deconstruct(
                out Tile tile, out Direction direction,
                out bool beforeIsConvex,  out bool afterIsConvex)
            {
                tile = Tile;
                direction = Direction;
                beforeIsConvex = BeforeIsConvex;
                afterIsConvex = AfterIsConvex;
            }
        }

        private readonly List<Part> edges;

        private TileAreaBorder(List<Part> edges)
        {
            this.edges = edges;
        }

        public static TileAreaBorder From(Level level, Func<Tile, bool> predicate)
            => From(Tilemap.EnumerateTilemapWith(level.Radius), predicate);

        public static TileAreaBorder From(IEnumerable<Tile> area, Func<Tile, bool> predicate)
            => From(area.Where(predicate));

        public static TileAreaBorder From(IEnumerable<Tile> area)
            => From(new HashSet<Tile>(area));

        public static TileAreaBorder From(HashSet<Tile> area)
        {
            var edges = area.SelectMany(
                    tile => Extensions.Directions
                        .Where(direction => !area.Contains(tile.Neighbour(direction)))
                        .Select(direction => new Part(
                            tile, direction,
                            !area.Contains(tile.Neighbour(direction.Previous())),
                            !area.Contains(tile.Neighbour(direction.Next()))
                            ))
                )
                .ToList();

            return new TileAreaBorder(edges);
        }

        public void Visit(Action<Part> visitBorder)
        {
            edges.ForEach(visitBorder);
        }
    }
}
