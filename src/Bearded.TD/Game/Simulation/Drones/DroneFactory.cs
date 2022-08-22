using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drones;

static class DroneFactory
{
    public static GameObject CreateDrone(ComponentOwnerBlueprint blueprint, Position3 position)
    {
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, null, position, Direction2.Zero);
        return obj;
    }
}
