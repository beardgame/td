using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingFactory
    {
        private readonly GameState gameState;

        public BuildingFactory(GameState gameState)
        {
            this.gameState = gameState;
        }

        // TODO: should return a ComponentGameObject instead
        public Building Create(
            Id<Building> id, IBuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
        {
            var building = new Building(blueprint);
            gameState.Add(building);
            if (!building.GetComponents<IEnemySink>().Any())
            {
                building.AddComponent(new BackupSink<Building>());
            }
            building.AddComponent(new AllowManualControl<Building>());
            building.AddComponent(new BuildingStateManager<Building>());
            building.AddComponent(new BuildingUpgradeManager<Building>());
            building.AddComponent(new DamageReceiver<Building>());
            building.AddComponent(new DamageSource<Building>());
            building.AddComponent(new FactionProvider<Building>(faction));
            building.AddComponent(new FootprintPosition<Building>());
            building.AddComponent(new IdProvider<Building>(id));
            building.AddComponent(new IncompleteBuilding<Building>());
            building.AddComponent(new ReportSubject<Building>());
            building.AddComponent(new Selectable<Building>());
            building.AddComponent(new StaticTileOccupation<Building>(footprint));
            building.AddComponent(new StatisticCollector<Building>());
            gameState.BuildingLayer.AddBuilding(building);
            building.Deleting += () => gameState.BuildingLayer.RemoveBuilding(building);
            return building;
        }

        public Building CreateGhost(
            IBuildingBlueprint blueprint, Faction faction, out MovableTileOccupation<Building> tileOccupation)
        {
            var ghost = new Building(blueprint);
            gameState.Add(ghost);
            ghost.AddComponent(new BuildingGhostDrawing<Building>());
            ghost.AddComponent(new GhostBuildingStateProvider<Building>());
            ghost.AddComponent(new FactionProvider<Building>(faction));
            ghost.AddComponent(new FootprintPosition<Building>());
            tileOccupation = new MovableTileOccupation<Building>();
            ghost.AddComponent(tileOccupation);
            return ghost;
        }
    }
}
