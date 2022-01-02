using System;
using static Bearded.TD.Tiles.Direction;

namespace Bearded.TD.Tiles;

readonly struct TileEdge// : IEquatable<TileEdge>
{
    private readonly Tile tile;
    private readonly Direction direction;

    public bool IsValid => direction != Unknown;

    public (Tile, Tile) AdjacentTiles =>
        direction is Right or UpRight or UpLeft
            ? (tile, tile.Neighbor(direction))
            : throw invalidDirectionException();

    public static TileEdge From(Tile tile, Direction direction)
    {
        normalize(ref tile, ref direction);

        return new(tile, direction);
    }

    private static void normalize(ref Tile tile, ref Direction direction)
    {
        if (direction is Left or DownLeft or DownRight)
        {
            tile = tile.Neighbor(direction);
            direction = direction.Opposite();
        }
    }

    private TileEdge(Tile tile, Direction direction)
    {
        this.tile = tile;
        this.direction = direction;
    }

    public TEdgeData GetEdgeFrom<T, TEdgeData>(Tilemap<T> tilemap)
        where T : ITileEdges<TEdgeData>
    {
        return direction switch
        {
            Right => tilemap[tile].Right,
            UpRight => tilemap[tile].UpRight,
            UpLeft => tilemap[tile].UpLeft,
            _ => throw invalidDirectionException()
        };
    }

    public void ModifyEdgeIn<T, TEdgeData>(Tilemap<T> tilemap, TEdgeData data)
        where T : IModifiableTileEdges<T, TEdgeData>
    {
        var edges = tilemap[tile];
        tilemap[tile] = direction switch
        {
            Right => edges.WithRight(data),
            UpRight => edges.WithUpRight(data),
            UpLeft => edges.WithUpLeft(data),
            _ => throw invalidDirectionException()
        };
    }

    public static bool operator ==(TileEdge left, TileEdge right) => left.Equals(right);
    public static bool operator !=(TileEdge left, TileEdge right) => !left.Equals(right);
    public override bool Equals(object? obj) => obj is TileEdge other && Equals(other);
    public bool Equals(TileEdge other) => tile.Equals(other.tile) && direction == other.direction;
    public override int GetHashCode() => HashCode.Combine(tile, (int) direction);

    private ArgumentOutOfRangeException invalidDirectionException() =>
        new (nameof(direction), direction, "Tile edge has invalid direction.");
}

interface ITileEdges<out TEdgeData>
{
    TEdgeData Right { get; }
    TEdgeData UpRight { get; }
    TEdgeData UpLeft { get; }
}

interface IModifiableTileEdges<out T, TEdgeData> : ITileEdges<TEdgeData>
    where T : IModifiableTileEdges<T, TEdgeData>
{
    T WithRight(TEdgeData data);
    T WithUpRight(TEdgeData data);
    T WithUpLeft(TEdgeData data);
}