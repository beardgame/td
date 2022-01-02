using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Navigation;

sealed class PassabilityLayer
{
    private readonly Level level;
    private readonly Tilemap<TilePassability> tilemap;
    private readonly ImmutableArray<Func<Tile, Tile, bool>> extraConditions;

    public PassabilityLayer(Level level, IEnumerable<Func<Tile, Tile, bool>> extraConditions)
    {
        this.level = level;
        this.extraConditions = ImmutableArray.CreateRange(extraConditions);
        tilemap = new Tilemap<TilePassability>(level.Radius);
    }

    // TODO: take buildings into account

    public bool HandleTilePassabilityChanged(Tile tile, bool isPassable)
    {
        var currentPassability = tilemap[tile];
        if (currentPassability.IsPassable == isPassable)
        {
            return false;
        }

        tilemap[tile] = currentPassability.WithPassability(isPassable);

        foreach (var dir in level.ValidDirectionsFrom(tile))
        {
            var neighbour = tile.Neighbor(dir);
            if (isPassable && extraConditions.All(c => c(tile, neighbour)))
            {
                openDirection(neighbour, dir.Opposite());
            }
            else
            {
                closeDirection(neighbour, dir.Opposite());
            }

            var neighbourIsPassable = tilemap[neighbour].IsPassable;
            if (neighbourIsPassable && extraConditions.All(c => c(neighbour, tile)))
            {
                openDirection(tile, dir);
            }
            else
            {
                closeDirection(tile, dir);
            }
        }

        return true;
    }

    private void closeDirection(Tile tile, Direction direction)
    {
        var passability = tilemap[tile];
        var passableDirections = passability.PassableDirections.Except(direction);
        tilemap[tile] = passability.WithPassableDirections(passableDirections);
    }

    private void openDirection(Tile tile, Direction direction)
    {
        var passability = tilemap[tile];
        var passableDirections = passability.PassableDirections.And(direction);
        tilemap[tile] = passability.WithPassableDirections(passableDirections);
    }

    public TilePassability this[Tile tile] => tilemap[tile];
}