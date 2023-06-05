using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingLayer
{
    private readonly GlobalGameEvents events;
    private readonly Dictionary<Tile, List<GameObject>> buildingLookup = new();

    public BuildingLayer(GlobalGameEvents events)
    {
        this.events = events;
    }

    public void AddBuilding(GameObject building)
    {
        foreach (var tile in building.GetTilePresence().OccupiedTiles)
        {
            getList(tile).Add(building);
        }
    }

    public void RemoveBuilding(GameObject building)
    {
        foreach (var tile in building.GetTilePresence().OccupiedTiles)
        {
            getList(tile).Remove(building);
        }
    }

    public bool HasMaterializedBuilding(Tile tile)
    {
        return buildingsAt(tile).Any(b => getStateFor(b) is { IsMaterialized: true });
    }

    public bool TryGetMaterializedBuilding(Tile tile, [NotNullWhen(true)] out GameObject? building)
    {
        foreach (var candidate in buildingsAt(tile))
        {
            if (getStateFor(candidate) is { IsMaterialized: true })
            {
                building = candidate;
                return true;
            }
        }

        building = null;
        return false;
    }

    public IEnumerable<GameObject> this[Tile tile] => buildingsAt(tile);

    private IEnumerable<GameObject> buildingsAt(Tile tile) =>
        buildingLookup.TryGetValue(tile, out var list) ? list : Enumerable.Empty<GameObject>();

    private List<GameObject> getList(Tile tile) => buildingLookup.GetValueOrInsertNewDefaultFor(tile);

    private static IBuildingState? getStateFor(GameObject building)
    {
        return building.GetComponents<IBuildingStateProvider>().SingleOrDefault()?.State;
    }
}
