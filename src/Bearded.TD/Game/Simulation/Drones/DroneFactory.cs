using System.Collections.Immutable;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drones;

static class DroneFactory
{
    public static GameObject CreateDrone(
        ComponentOwnerBlueprint blueprint,
        Position3 position,
        Faction faction,
        DroneRequest request,
        ImmutableArray<Direction> path,
        out Drone droneComp)
    {
        droneComp = new Drone(request, path);

        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, null, position, Direction2.Zero);
        obj.AddComponent(new FactionProvider(faction));
        obj.AddComponent(droneComp);
        return obj;
    }
}
