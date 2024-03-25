using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Debug;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

static class BuildingFactory
{
    public static GameObject Create(
        Id<GameObject> id, IGameObjectBlueprint blueprint, Faction faction,
        PositionedFootprint footprint)
    {
        var building = GameObjectFactory.CreateFromBlueprintWithoutRenderer(
            blueprint, null, footprint.CenterPosition.WithZ(0), Direction2.Zero);
        if (!building.GetComponents<IEnemySink>().Any())
        {
            building.AddComponent(new BackupSink());
        }

        var statuses = new StatusTracker();

        building.AddComponent(new GhostBuildingRenderer());
        building.AddComponent(new BuildingStateManager());
        building.AddComponent(new BuildingUpgradeManager());
        building.AddComponent(new DamageSource());
        building.AddComponent(new DebugInvulnerable());
        building.AddComponent(new ElementSystemEntity());
        building.AddComponent(new FactionProvider(faction));
        building.AddComponent(new FootprintPosition());
        building.AddComponent(new HealthEventReceiver());
        building.AddComponent(new IdProvider(id));
        building.AddComponent(new IncompleteBuildingComponent());
        building.AddComponent(new Killable());
        building.AddComponent(new ReportSubject());
        building.AddComponent(new Selectable());
        building.AddComponent(new StaticFootprintTileNotifier(footprint));
        building.AddComponent(new DamageStatisticForwarder());
        building.AddComponent(statuses);
        building.AddComponent(new StatusRenderer(statuses, new BuildingStatusDisplayCondition()));
        building.AddComponent(new TemperatureProperty());
        building.AddComponent(new TileBasedVisibility());
        building.AddComponent(new EventReceiver<TakeHit>());
#if DEBUG
        building.AddComponent(new DebugReporter());
#endif
        return building;
    }

    public static GameObject CreateGhost(
        IGameObjectBlueprint blueprint, Faction faction,
        out DynamicFootprintTileNotifier tileNotifier)
    {
        var ghost = GameObjectFactory.CreateFromBlueprintWithoutRenderer(
            blueprint, null, Position3.Zero, Direction2.Zero);
        ghost.AddComponent(new GhostBuildingRenderer());
        ghost.AddComponent(new BuildingGhostDrawing());
        ghost.AddComponent(new GhostBuildingStateProvider());
        ghost.AddComponent(new FactionProvider(faction));
        ghost.AddComponent(new FootprintPosition());
        tileNotifier = new DynamicFootprintTileNotifier();
        ghost.AddComponent(tileNotifier);
        return ghost;
    }

    private sealed class BuildingStatusDisplayCondition : InputAwareStatusDisplayCondition
    {
        private IBuildingStateProvider? stateProvider;

        public override bool ShouldDraw => base.ShouldDraw && (stateProvider?.State.IsCompleted ?? true);

        public override void Activate(GameObject owner, ComponentEvents events)
        {
            owner.TryGetSingleComponent(out stateProvider);
            base.Activate(owner, events);
        }
    }
}
