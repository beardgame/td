using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration;

static class ExplorationBeaconFactory
{
    public static ComponentGameObject CreateExplorationBeacon(
        GameState game, ComponentOwnerBlueprint blueprint, Tile tile, Zone zone)
    {
        var obj = ComponentGameObjectFactory.CreateFromBlueprintWithDefaultRenderer(
            game, blueprint, null, Level.GetPosition(tile).WithZ(0));

        obj.AddComponent(new AlwaysVisibleVisibility<ComponentGameObject>());
        obj.AddComponent(new DrawZoneOnSelect<ComponentGameObject>());
        obj.AddComponent(new ReportSubject<ComponentGameObject>());
        obj.AddComponent(new Selectable<ComponentGameObject>());
        obj.AddComponent(
            new StaticTileOccupation<ComponentGameObject>(blueprint.GetFootprintGroup().Positioned(0, tile)));
        obj.AddComponent(new ZoneRevealer<ComponentGameObject>(zone));

        return obj;
    }
}
