using Bearded.TD.Content.Models;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Workers;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("workerAntenna")]
    sealed class WorkerAntenna<T> : Component<T, IWorkerAntennaParameters>, IWorkerAntenna
        where T : GameObject, IFactioned, IPositionable
    {
        private Building ownerAsBuilding;
        private bool isInitialised;

        public Position2 Position => Owner.Position;

        public Unit WorkerRange { get; private set; }

        public WorkerAntenna(IWorkerAntennaParameters parameters) : base(parameters) {}

        protected override void Initialise()
        {
            ownerAsBuilding = Owner as Building;
            if (ownerAsBuilding?.IsCompleted ?? true) initialiseInternal();
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
                if (!ownerAsBuilding.IsCompleted) return;
                initialiseInternal();
            }

            if (Parameters.WorkerRange != WorkerRange)
            {
                WorkerRange = Parameters.WorkerRange;
                Owner.Faction.WorkerNetwork.OnAntennaRangeUpdated(Owner.Game);
            }
        }

        public override void Draw(GeometryManager geometries) {}
    }
}
