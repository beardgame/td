using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Drones;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

sealed partial class FuelSystem
{
    private DronePreview? preview;

    private void enablePreview()
    {
        if (preview is not null || factionProvider is null || buildingStateProvider is null)
        {
            return;
        }

        preview = new DronePreview(
            Owner,
            Owner.Game.Meta.Blueprints.GameObjects[ModAwareId.ForDefaultMod("dronePathPreview")],
            factionProvider,
            buildingStateProvider);
    }

    private void disablePreview()
    {
        preview?.Terminate();
        preview = null;
    }

    private sealed class DronePreview
    {
        private static readonly TimeSpan ghostSpawnDelay = 1.S();

        private readonly GameObject owner;
        private readonly IGameObjectBlueprint blueprint;
        private readonly IFactionProvider factionProvider;
        private readonly IBuildingStateProvider buildingStateProvider;
        private Tile lastKnownTile;
        private Instant lastKnownMove;
        private bool hasValidPreview;
        private DroneFulfillmentPreview fulfillmentPreview;
        private GameObject? pathPreview;
        private DroneFulfillment? ghostPreview;

        public DronePreview(
            GameObject owner,
            IGameObjectBlueprint blueprint,
            IFactionProvider factionProvider,
            IBuildingStateProvider buildingStateProvider)
        {
            this.owner = owner;
            this.blueprint = blueprint;
            this.factionProvider = factionProvider;
            this.buildingStateProvider = buildingStateProvider;
            lastKnownTile = new Tile(int.MinValue, int.MinValue);
        }

        public void Update()
        {
            var drawing = buildingStateProvider.State.RangeDrawing;
            var ownerTile = Level.GetTile(owner.Position);

            var drawPath = drawing != RangeDrawStyle.DoNotDraw;
            var drawGhost = drawing == RangeDrawStyle.DrawFull;
            var moved = lastKnownTile != ownerTile;

            if (moved)
            {
                lastKnownTile = ownerTile;
                lastKnownMove = owner.Game.Time;
                refreshFulfillmentPreview();
            }

            updatePath(drawPath && hasValidPreview, moved);
            updateGhost(drawGhost && hasValidPreview, moved);
        }

        public void Terminate()
        {
            pathPreview?.Delete();
            pathPreview = null;
            ghostPreview?.Cancel();
            ghostPreview = null;
        }

        private void refreshFulfillmentPreview()
        {
            if (!owner.Game.Level.IsValid(lastKnownTile))
            {
                fulfillmentPreview = default;
                hasValidPreview = false;
            }

            var request = DroneRequest.Noop(lastKnownTile);
            var requestEvent = new RequestDrone(factionProvider.Faction, request, null);
            owner.Game.Meta.Events.Preview(ref requestEvent);

            if (requestEvent.FulfillmentPreview is { } p)
            {
                fulfillmentPreview = p;
                hasValidPreview = true;
                return;
            }

            fulfillmentPreview = default;
            hasValidPreview = false;
        }

        private void updatePath(bool draw, bool moved)
        {
            if (!draw && pathPreview is not null)
            {
                pathPreview?.Delete();
                pathPreview = null;
            }

            if (draw && (moved || pathPreview is null))
            {
                spawnNewPathPreview();
            }
        }

        private void spawnNewPathPreview()
        {
            pathPreview?.Delete();
            pathPreview = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, owner, owner.Position);
            pathPreview.AddComponent(
                new Property<PrecalculatedPath>(
                    PrecalculatedPath.FromPathfindingResult(
                        fulfillmentPreview.Spawner.Location, fulfillmentPreview.Path)));
            owner.Game.Add(pathPreview);
        }

        private void updateGhost(bool draw, bool moved)
        {
            if ((!draw || moved) && ghostPreview is not null)
            {
                ghostPreview.Cancel();
                ghostPreview = null;
            }

            if (draw && ghostPreview is null && owner.Game.Time - lastKnownMove >= ghostSpawnDelay)
            {
                spawnGhostPreview();
            }
        }

        private void spawnGhostPreview()
        {
            ghostPreview = fulfillmentPreview.Spawner.Preview(fulfillmentPreview);
        }
    }
}
