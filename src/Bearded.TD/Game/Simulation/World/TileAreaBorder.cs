using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.World;

sealed class TileAreaBorder
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

    private readonly ImmutableArray<Part> edges;

    public bool IsEmpty => edges.Length == 0;

    private TileAreaBorder(ImmutableArray<Part> edges)
    {
        this.edges = edges;
    }

    public static TileAreaBorder From(Level level, Func<Tile, bool> predicate)
        => From(Tilemap.EnumerateTilemapWith(level.Radius), predicate);

    public static TileAreaBorder From(IEnumerable<Tile> area, Func<Tile, bool> predicate)
        => From(area.Where(predicate));

    public static TileAreaBorder From(IArea area)
        => From(area.ToImmutableHashSet());

    public static TileAreaBorder From(IEnumerable<Tile> area)
        => From(area.ToImmutableHashSet());

    public static TileAreaBorder From(ImmutableHashSet<Tile> area)
    {
        var edges = area.SelectMany(
                tile => Extensions.Directions
                    .Where(direction => !area.Contains(tile.Neighbor(direction)))
                    .Select(direction => new Part(
                        tile, direction,
                        !area.Contains(tile.Neighbor(direction.Previous())),
                        !area.Contains(tile.Neighbor(direction.Next()))
                    ))
            )
            .ToImmutableArray();

        return new TileAreaBorder(edges);
    }

    public void Visit(Action<Part> visitBorder)
    {
        edges.ForEach(visitBorder);
    }
}