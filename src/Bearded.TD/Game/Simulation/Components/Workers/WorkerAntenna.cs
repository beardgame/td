using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.InGameUI;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Workers
{
    [Component("workerAntenna")]
    sealed class WorkerAntenna<T> : Component<T, IWorkerAntennaParameters>, IWorkerAntenna
        where T : GameObject, IFactioned, IPositionable
    {
        private Building? ownerAsBuilding;
        private WorkerNetwork? workerNetwork;
        private bool isInitialized;

        public Position2 Position => Owner.Position.XY();

        public Unit WorkerRange { get; private set; }

        public WorkerAntenna(IWorkerAntennaParameters parameters) : base(parameters) {}

        protected override void Initialize()
        {
            ownerAsBuilding = Owner as Building;
            Owner.Faction.TryGetBehaviorIncludingAncestors(out workerNetwork);
            initializeIfOwnerIsCompletedBuilding();
        }

        private void initializeIfOwnerIsCompletedBuilding()
        {
            if (ownerAsBuilding?.IsCompleted ?? false)
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
            if (!(Owner is ISelectable selectable && selectable.SelectionState != SelectionState.Default))
                return;

            var alpha = (selectable.SelectionState == SelectionState.Selected ? 0.5f : 0.25f);

            var networkBorder =
                TileAreaBorder.From(Owner.Game.Level, t => workerNetwork?.IsInRange(t) ?? false);

            TileAreaBorderRenderer.Render(networkBorder, Owner.Game, Color.DodgerBlue * alpha);

            var localBorder = TileAreaBorder.From(((IWorkerAntenna) this).Coverage);

            TileAreaBorderRenderer.Render(localBorder, Owner.Game, Color.Orange * alpha);
        }
    }
}
