using Bearded.TD.Content.Models;
using Bearded.TD.Game.Workers;
using Bearded.TD.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components.Workers
{
    [Component("workerHub")]
    sealed class WorkerHub<T> : Component<T, IWorkerHubParameters>
        where T : GameObject, IFactioned
    {
        private int numWorkersActive;

        public WorkerHub(IWorkerHubParameters parameters) : base(parameters) { }

        protected override void Initialise() {}

        public override void Update(TimeSpan elapsedTime)
        {
            while (numWorkersActive < Parameters.NumWorkers)
            {
                addNewWorker();
            }
        }

        public override void Draw(GeometryManager geometries) { }

        private void addNewWorker()
        {
            Owner.Game.Add(new Worker(Owner));
            numWorkersActive++;
        }
    }
}
