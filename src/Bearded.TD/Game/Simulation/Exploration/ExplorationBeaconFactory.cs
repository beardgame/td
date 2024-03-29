using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration;

static class ExplorationBeaconFactory
{
    public static GameObject CreateExplorationBeacon(GameObjectBlueprint blueprint, Tile tile, Zone zone)
    {
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(
            blueprint, null, Level.GetPosition(tile).WithZ(0));

        obj.AddComponent(new ConditionBasedVisibility(o => o.Game.ExplorationManager.HasExplorationToken));
        obj.AddComponent(new DrawZoneOnSelect());
        obj.AddComponent(new ReportSubject());
        obj.AddComponent(new Selectable());
        obj.AddComponent(
            new StaticFootprintTileNotifier(blueprint.GetFootprint().Positioned(tile, Orientation.Default)));
        obj.AddComponent(new ZoneRevealer(zone));

        return obj;
    }
}
