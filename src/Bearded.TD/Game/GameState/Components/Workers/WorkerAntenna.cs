using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.GameState.Buildings;
using Bearded.TD.Game.GameState.Factions;
using Bearded.TD.Game.GameState.Workers;
using Bearded.TD.Game.GameState.World;
using Bearded.TD.Game.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.InGameUI;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameState.Components.Workers
{
    [Component("workerAntenna")]
    sealed class WorkerAntenna<T> : Component<T, IWorkerAntennaParameters>, IWorkerAntenna
        where T : GameObject, IFactioned, IPositionable
    {
        private Building ownerAsBuilding;
        private bool isInitialised;

        public Position2 Position => Owner.Position.XY();

        public Unit WorkerRange { get; private set; }

        public WorkerAntenna(IWorkerAntennaParameters parameters) : base(parameters) {}

        protected override void Initialise()
        {
            ownerAsBuilding = Owner as Building;
            initialiseIfOwnerIsCompletedBuilding();
        }

        private void initialiseIfOwnerIsCompletedBuilding()
        {
            if (ownerAsBuilding?.IsCompleted ?? false)
                initialiseInternal();
        }

        private void initialiseInternal()
        {
            WorkerRange = Parameters.WorkerRange;
            Owner.Faction.WorkerNetwork.RegisterAntenna(Owner.Game, this);
            Owner.Deleting += () => Owner.Faction.WorkerNetwork.UnregisterAntenna(Owner.Game, this);
            isInitialised = true;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (!isInitialised)
            {
                initialiseIfOwnerIsCompletedBuilding();
            }

            if (Parameters.WorkerRange != WorkerRange)
            {
                WorkerRange = Parameters.WorkerRange;
                Owner.Faction.WorkerNetwork.OnAntennaRangeUpdated(Owner.Game);
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            if (!(Owner is ISelectable selectable && selectable.SelectionState != SelectionState.Default))
                return;

            var alpha = (selectable.SelectionState == SelectionState.Selected ? 0.5f : 0.25f);

            var workerNetwork = Owner.Faction.WorkerNetwork;
            var networkBorder = TileAreaBorder.From(Owner.Game.Level, workerNetwork.IsInRange);

            TileAreaBorderRenderer.Render(Owner.Game, networkBorder, Color.DodgerBlue * alpha);

            var localArea = Tilemap
                .GetSpiralCenteredAt(Level.GetTile(Position), (int) Parameters.WorkerRange.NumericValue + 1)
                .Where(t => WorkerNetwork.IsTileInAntennaRange(this, Level.GetPosition(t)));

            var localBorder = TileAreaBorder.From(localArea);

            TileAreaBorderRenderer.Render(Owner.Game, localBorder, Color.Orange * alpha);
        }
    }
}
