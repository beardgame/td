using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingPlacementLayer
{
    private readonly Level level;
    private readonly GeometryLayer geometryLayer;
    private readonly BuildingLayer buildingLayer;
    private readonly Lazy<PassabilityLayer> walkablePassability;
    private readonly HashSet<Tile> blockedTiles = new HashSet<Tile>();

    public BuildingPlacementLayer(
        Level level,
        GeometryLayer geometryLayer,
        BuildingLayer buildingLayer,
        Lazy<PassabilityLayer> walkablePassability)
    {
        this.level = level;
        this.geometryLayer = geometryLayer;
        this.buildingLayer = buildingLayer;
        this.walkablePassability = walkablePassability;
    }

    public void BlockTileForBuilding(Tile tile) => blockedTiles.Add(tile);

    public bool IsFootprintValidForBuilding(PositionedFootprint footprint)
        => footprint.OccupiedTiles.All(IsTileValidForBuilding) && allAdjacentTilesAreWalkable(footprint.OccupiedTiles);

    public bool IsTileValidForBuilding(Tile tile)
    {
        return level.IsValid(tile)
            && !blockedTiles.Contains(tile)
            && geometryLayer[tile].Type == TileType.Floor
            && buildingLayer.GetOccupationFor(tile) == BuildingLayer.Occupation.None;
    }

    private bool allAdjacentTilesAreWalkable(IEnumerable<Tile> tiles)
    {
        var tilesSet = new HashSet<Tile>(tiles);

        foreach (var tile in tilesSet)
        {
            if (tile.PossibleNeighbours().Where(tilesSet.Contains)
                .Any(t => !IsTileCombinationValidForBuilding(tile, t)))
                return false;
        }

        return true;
    }

    public bool IsTileCombinationValidForBuilding(Tile t0, Tile t1)
    {
        var tileDistance = t0.DistanceTo(t1);
        if (tileDistance != 1)
            throw new InvalidOperationException("Tiles must be adjacent.");

        var direction = Directions.All.Enumerate().First(d => t0.Neighbor(d) == t1);

        return walkablePassability.Value[t0].PassableDirections.Includes(direction)
            && walkablePassability.Value[t1].PassableDirections.Includes(direction.Opposite());
    }
}