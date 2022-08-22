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
        this IDroneSpawner spawner, DroneRequest request, out DroneFulfillmentPreview preview)
    {
        var path = Pathfinder.Default.FindPath(spawner.Location, request.Location);
        if (path == null)
        {
            preview = default;
            return false;
        }

        preview = new DroneFulfillmentPreview(spawner, path);
        return true;
    }
}
