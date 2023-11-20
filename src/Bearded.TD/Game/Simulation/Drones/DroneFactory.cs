using Bearded.TD.Game.Simulation.Drawing;
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
        IGameObjectBlueprint blueprint,
        Position3 position,
        Faction faction,
        DroneRequest request,
        PrecalculatedPath path,
        out Drone droneComp)
    {
        var obj = createDroneWithoutRenderer(
            blueprint,
            position,
            faction,
            request,
            path,
            out droneComp);
        obj.AddComponent(new DefaultComponentRenderer());
        return obj;
    }

    public static GameObject CreateDroneGhost(
        IGameObjectBlueprint blueprint,
        Position3 position,
        Faction faction,
        PrecalculatedPath path,
        out Drone droneComp)
    {
        var obj = createDroneWithoutRenderer(
            blueprint,
            position,
            faction,
            DroneRequest.Noop(path.GoalTile),
            path,
            out droneComp);
        obj.AddComponent(new GhostBuildingRenderer());
        return obj;
    }

    private static GameObject createDroneWithoutRenderer(
        IGameObjectBlueprint blueprint,
        Position3 position,
        Faction faction,
        DroneRequest request,
        PrecalculatedPath path,
        out Drone droneComp)
    {
        Argument.Satisfies(request.Location == path.GoalTile);

        droneComp = new Drone(request, path);

        var obj = GameObjectFactory.CreateFromBlueprintWithoutRenderer(blueprint, null, position, Direction2.Zero);
        obj.AddComponent(new FactionProvider(faction));
        obj.AddComponent(droneComp);
        return obj;
    }
}
