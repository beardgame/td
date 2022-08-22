using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Drones;

static class DroneFactory
{
    public static GameObject CreateDrone(
        IComponentOwnerBlueprint blueprint,
        Position3 position,
        Faction faction,
        DroneRequest request,
        PrecalculatedPath path,
        out Drone droneComp)
    {
        Argument.Satisfies(request.Location == path.GoalTile);

        droneComp = new Drone(request, path);

        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, null, position, Direction2.Zero);
        obj.AddComponent(new FactionProvider(faction));
        obj.AddComponent(droneComp);
        return obj;
    }
}
