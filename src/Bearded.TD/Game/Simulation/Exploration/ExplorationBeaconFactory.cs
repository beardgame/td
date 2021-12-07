using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration
{
    static class ExplorationBeaconFactory
    {
        public static ComponentGameObject CreateExplorationBeacon(GameState game, Tile tile, Zone zone)
        {
            var obj =
                ComponentGameObjectFactory.CreateWithDefaultRenderer(game, null, Level.GetPosition(tile).WithZ(0));

            obj.AddComponent(new AlwaysVisibleVisibility<ComponentGameObject>());
            obj.AddComponent(new ReportSubject<ComponentGameObject>());
            obj.AddComponent(new Selectable<ComponentGameObject>());
            // TODO: add sprite
            obj.AddComponent(new StaticTileOccupation<ComponentGameObject>(FootprintGroup.Single.Positioned(0, tile)));
            obj.AddComponent(new ZoneRevealer<ComponentGameObject>(zone));

            return obj;
        }
    }
}
