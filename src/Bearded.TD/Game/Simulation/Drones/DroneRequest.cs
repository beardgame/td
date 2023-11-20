using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Drones;

readonly record struct DroneRequest(Tile Location, DroneAction Action)
{
    public static DroneRequest Noop(Tile location) => new(location, () => { });
}

delegate void DroneAction();
