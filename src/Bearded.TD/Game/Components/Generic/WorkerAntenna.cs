using Bearded.TD.Content.Models;
using Bearded.TD.Game.Workers;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("workerAntenna")]
    sealed class WorkerAntenna<T> : Component<T, IWorkerAntennaParameters>, IWorkerAntenna
        where T : GameObject, IFactioned, IPositionable
    {
        public Position2 Position => Owner.Position;

        public Unit WorkerRange { get; private set; }

        public WorkerAntenna(IWorkerAntennaParameters parameters) : base(parameters) {}

        protected override void Initialise()
        {
            WorkerRange = Parameters.WorkerRange;
            Owner.Faction.WorkerNetwork.RegisterAntenna(Owner.Game, this);
            Owner.Deleting += () => Owner.Faction.WorkerNetwork.UnregisterAntenna(Owner.Game, this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Parameters.WorkerRange != WorkerRange)
            {
                WorkerRange = Parameters.WorkerRange;
                Owner.Faction.WorkerNetwork.OnAntennaRangeUpdated(Owner.Game);
            }
        }

        public override void Draw(GeometryManager geometries) {}
    }
}
