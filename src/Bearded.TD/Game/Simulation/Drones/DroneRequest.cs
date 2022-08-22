using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Drones;

readonly record struct DroneRequest(Tile Location, DroneAction Action);

delegate void DroneAction();
