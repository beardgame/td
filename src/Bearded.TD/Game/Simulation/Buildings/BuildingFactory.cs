using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Debug;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Exploration;
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

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingFactory
{
    private readonly GameState gameState;

    public BuildingFactory(GameState gameState)
    {
        this.gameState = gameState;
    }

    public GameObject Create(
        Id<GameObject> id, IComponentOwnerBlueprint blueprint, Faction faction,
        PositionedFootprint footprint)
    {
        var building = ComponentGameObjectFactory.CreateFromBlueprintWithoutRenderer(
            gameState, blueprint, null, Position3.Zero, Direction2.Zero);
        if (!building.GetComponents<IEnemySink>().Any())
        {
            building.AddComponent(new BackupSink());
        }

        building.AddComponent(new GhostBuildingRenderer());
        building.AddComponent(new AllowManualControl());
        building.AddComponent(new BuildingStateManager());
        building.AddComponent(new BuildingUpgradeManager());
        building.AddComponent(new DamageSource());
        building.AddComponent(new DebugInvulnerable());
        building.AddComponent(new FactionProvider(faction));
        building.AddComponent(new FootprintPosition());
        building.AddComponent(new HealthBar());
        building.AddComponent(new HealthEventReceiver());
        building.AddComponent(new IdProvider(id));
        building.AddComponent(new IncompleteBuilding());
        building.AddComponent(new ReportSubject());
        building.AddComponent(new Selectable());
        building.AddComponent(new StaticTileOccupation(footprint));
        building.AddComponent(new StatisticCollector());
        building.AddComponent(new TileBasedVisibility());
#if DEBUG
        building.AddComponent(new DebugReporter());
#endif
        gameState.BuildingLayer.AddBuilding(building);
        building.Deleting += () => gameState.BuildingLayer.RemoveBuilding(building);
        return building;
    }

    public GameObject CreateGhost(
        IComponentOwnerBlueprint blueprint, Faction faction,
        out MovableTileOccupation tileOccupation)
    {
        var ghost = ComponentGameObjectFactory.CreateFromBlueprintWithoutRenderer(
            gameState, blueprint, null, Position3.Zero, Direction2.Zero);
        ghost.AddComponent(new GhostBuildingRenderer());
        ghost.AddComponent(new BuildingGhostDrawing());
        ghost.AddComponent(new GhostBuildingStateProvider());
        ghost.AddComponent(new FactionProvider(faction));
        ghost.AddComponent(new FootprintPosition());
        tileOccupation = new MovableTileOccupation();
        ghost.AddComponent(tileOccupation);
        return ghost;
    }
}
