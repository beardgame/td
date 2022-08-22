using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Drones;

interface IDroneSpawner
{
    Tile Location { get; }
    DroneFulfillment Fulfill(DroneRequest request, DroneFulfillmentPreview preview);
}

static class DroneSpawnerExtensions
{
    public static bool TryFulfillRequest(
        this IDroneSpawner spawner, PassabilityLayer passability, DroneRequest request, out DroneFulfillmentPreview preview)
    {
        var pathFinder = Pathfinder.WithTileCosts(t => passability[t].IsPassable ? 1 : null, 1);
        var path = pathFinder.FindPath(spawner.Location, request.Location);
        if (path == null)
        {
            preview = default;
            return false;
        }

        preview = new DroneFulfillmentPreview(spawner, path);
        return true;
    }
}
