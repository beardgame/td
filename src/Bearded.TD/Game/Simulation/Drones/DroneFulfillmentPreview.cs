using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Drones;

readonly record struct DroneFulfillmentPreview(IDroneSpawner Spawner, Pathfinder.Result Path);
