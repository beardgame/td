using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Rendering;
using Bearded.Utilities.Geometry;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Workers
{
    [Component("workerHub")]
    sealed class WorkerHub<T> : Component<T, IWorkerHubParameters>
        where T : GameObject, IComponentOwner, IFactioned, IPositionable
    {
        private int numWorkersActive;

        public WorkerHub(IWorkerHubParameters parameters) : base(parameters) { }

        protected override void Initialize() { }

        public override void Update(TimeSpan elapsedTime)
        {
            while (numWorkersActive < Parameters.NumWorkers)
            {
                addNewWorker();
            }
        }

        public override void Draw(CoreDrawers drawers) { }

        private void addNewWorker()
        {
            var obj = new ComponentGameObject(Parameters.Drone, Owner, Owner.Position, Direction2.Zero);

            Owner.Game.Add(obj);
            numWorkersActive++;
        }
    }
}
