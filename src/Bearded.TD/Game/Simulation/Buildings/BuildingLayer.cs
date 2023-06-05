using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingLayer : ObjectLayer
{
    public bool HasMaterializedBuilding(Tile tile) => GetObjectsOnTile(tile).Any(isMaterialized);

    public bool TryGetMaterializedBuilding(Tile tile, [NotNullWhen(true)] out GameObject? building)
    {
        foreach (var candidate in GetObjectsOnTile(tile))
        {
            if (isMaterialized(candidate))
            {
                building = candidate;
                return true;
            }
        }

        building = null;
        return false;
    }

    private static bool isMaterialized(GameObject building) => getStateFor(building) is { IsMaterialized: true };

    private static IBuildingState? getStateFor(GameObject building) =>
        building.GetComponents<IBuildingStateProvider>().SingleOrDefault()?.State;
}
