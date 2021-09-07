using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Workers
{
    [Component("workerAntenna")]
    sealed class WorkerAntenna<T> : Component<T, IWorkerAntennaParameters>, IWorkerAntenna
        where T : GameObject, IFactioned, IPositionable
    {
        private IBuilding? ownerAsBuilding;
        private WorkerNetwork? workerNetwork;
        private bool isInitialized;
        private TileRangeDrawer? fullNetworkDrawer;
        private TileRangeDrawer? localNetworkDrawer;

        public Position2 Position => Owner.Position.XY();

        public Unit WorkerRange { get; private set; }

        public WorkerAntenna(IWorkerAntennaParameters parameters) : base(parameters) {}

        protected override void Initialize()
        {
            ownerAsBuilding = Owner as IBuilding;
            Owner.Faction.TryGetBehaviorIncludingAncestors(out workerNetwork);
            initializeIfOwnerIsCompletedBuilding();

            if (ownerAsBuilding != null)
            {
                fullNetworkDrawer = new TileRangeDrawer(
                    Owner.Game,
                    () => ownerAsBuilding!.State.RangeDrawing,
                    () => TileAreaBorder.From(Owner.Game.Level, t => workerNetwork?.IsInRange(t) ?? false),
                    Color.DodgerBlue);

                localNetworkDrawer = new TileRangeDrawer(
                    Owner.Game,
                    () => ownerAsBuilding!.State.RangeDrawing,
                    () => TileAreaBorder.From(((IWorkerAntenna) this).Coverage),
                    Color.Orange);
            }
        }

        private void initializeIfOwnerIsCompletedBuilding()
        {
            if (ownerAsBuilding?.State.IsFunctional ?? false)
                initializeInternal();
        }

        private void initializeInternal()
        {
            if (workerNetwork == null)
            {
                Owner.Game.Meta.Logger.Debug?.Log(
                    "Initializing worker antenna for building without faction is a no-op.");
            }

            WorkerRange = Parameters.WorkerRange;
            workerNetwork?.RegisterAntenna(this);
            Owner.Deleting += () => workerNetwork?.UnregisterAntenna(this);
            isInitialized = true;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            // TODO: this can probably be done event based
            if (!isInitialized)
            {
                initializeIfOwnerIsCompletedBuilding();
            }

            if (Parameters.WorkerRange != WorkerRange)
            {
                WorkerRange = Parameters.WorkerRange;
                workerNetwork?.OnAntennaRangeUpdated();
            }
        }

        public override void Draw(CoreDrawers drawers)
        {
            fullNetworkDrawer?.Draw();
            localNetworkDrawer?.Draw();
        }
    }
}
