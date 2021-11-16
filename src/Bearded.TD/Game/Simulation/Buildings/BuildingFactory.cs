using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingFactory
    {
        private readonly GameState gameState;

        public BuildingFactory(GameState gameState)
        {
            this.gameState = gameState;
        }

        public ComponentGameObject Create(
            Id<ComponentGameObject> id, IComponentOwnerBlueprint blueprint, Faction faction,
            PositionedFootprint footprint)
        {
            var building = ComponentGameObjectFactory.CreateWithDefaultRenderer(
                gameState, blueprint, null, Position3.Zero, Direction2.Zero);
            if (!building.GetComponents<IEnemySink>().Any())
            {
                building.AddComponent(new BackupSink<ComponentGameObject>());
            }

            building.AddComponent(new AllowManualControl<ComponentGameObject>());
            building.AddComponent(new BuildingStateManager<ComponentGameObject>());
            building.AddComponent(new BuildingUpgradeManager<ComponentGameObject>());
            building.AddComponent(new DamageSource<ComponentGameObject>());
            building.AddComponent(new DebugInvulnerable<ComponentGameObject>());
            building.AddComponent(new FactionProvider<ComponentGameObject>(faction));
            building.AddComponent(new FootprintPosition());
            building.AddComponent(new HealthEventReceiver<ComponentGameObject>());
            building.AddComponent(new IdProvider<ComponentGameObject>(id));
            building.AddComponent(new IncompleteBuilding<ComponentGameObject>());
            building.AddComponent(new ReportSubject<ComponentGameObject>());
            building.AddComponent(new Selectable<ComponentGameObject>());
            building.AddComponent(new StaticTileOccupation<ComponentGameObject>(footprint));
            building.AddComponent(new StatisticCollector<ComponentGameObject>());
            gameState.BuildingLayer.AddBuilding(building);
            building.Deleting += () => gameState.BuildingLayer.RemoveBuilding(building);
            return building;
        }

        public ComponentGameObject CreateGhost(
            IComponentOwnerBlueprint blueprint, Faction faction,
            out MovableTileOccupation<ComponentGameObject> tileOccupation)
        {
            var ghost = ComponentGameObjectFactory.CreateWithoutRenderer(
                gameState, blueprint, null, Position3.Zero, Direction2.Zero);
            ghost.AddComponent(new GhostBuildingRenderer<ComponentGameObject>());
            ghost.AddComponent(new BuildingGhostDrawing<ComponentGameObject>());
            ghost.AddComponent(new GhostBuildingStateProvider<ComponentGameObject>());
            ghost.AddComponent(new FactionProvider<ComponentGameObject>(faction));
            ghost.AddComponent(new FootprintPosition());
            tileOccupation = new MovableTileOccupation<ComponentGameObject>();
            ghost.AddComponent(tileOccupation);
            return ghost;
        }
    }
}
