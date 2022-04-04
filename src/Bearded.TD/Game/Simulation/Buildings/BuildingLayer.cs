using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingLayer
{
    public enum Occupation
    {
        None,
        ReservedForBuilding, // a building is scheduled to be built in this tile, but hasn't yet
        MaterializedBuilding
    }

    private readonly GlobalGameEvents events;
    private readonly Dictionary<Tile, GameObject> buildingLookup = new();

    public BuildingLayer(GlobalGameEvents events)
    {
        this.events = events;
    }

    public void AddBuilding(GameObject building)
    {
        foreach (var tile in OccupiedTileAccumulator.AccumulateOccupiedTiles(building))
        {
            State.Satisfies(!buildingLookup.ContainsKey(tile));
            buildingLookup.Add(tile, building);
        }
    }

    public void RemoveBuilding(GameObject building)
    {
        foreach (var tile in OccupiedTileAccumulator.AccumulateOccupiedTiles(building))
        {
            State.Satisfies(buildingLookup.ContainsKey(tile) && buildingLookup[tile] == building);
            buildingLookup.Remove(tile);
        }

        if (building.GetComponents<IBuildingStateProvider>().SingleOrDefault()?.State is { IsMaterialized: true })
        {
            events.Send(new BuildingDestroyed(building));
        }
    }

    public Occupation GetOccupationFor(Tile tile)
    {
        var state = getBuildingStateFor(tile);
        return state switch
        {
            null => Occupation.None,
            {IsMaterialized: true} => Occupation.MaterializedBuilding,
            _ => Occupation.ReservedForBuilding
        };
    }

    public bool TryGetMaterializedBuilding(Tile tile, [NotNullWhen(true)] out GameObject? building)
    {
        if (GetBuildingFor(tile) is { } candidate && getStateFor(candidate) is { IsMaterialized: true })
        {
            building = candidate;
            return true;
        }

        building = null;
        return false;
    }

    private IBuildingState? getBuildingStateFor(Tile tile)
    {
        return GetBuildingFor(tile) is { } building ? getStateFor(building) : null;
    }

    public GameObject? GetBuildingFor(Tile tile)
    {
        buildingLookup.TryGetValue(tile, out var building);
        return building;
    }

    public GameObject? this[Tile tile] => GetBuildingFor(tile);

    private static IBuildingState? getStateFor(IComponentOwner building)
    {
        return building.GetComponents<IBuildingStateProvider>().SingleOrDefault()?.State;
    }
}
