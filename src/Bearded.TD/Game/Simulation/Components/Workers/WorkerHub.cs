﻿using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Workers
{
    [Component("workerHub")]
    sealed class WorkerHub<T> : Component<T, IWorkerHubParameters>
        where T : GameObject, IFactioned
    {
        private int numWorkersActive;

        public WorkerHub(IWorkerHubParameters parameters) : base(parameters) { }

        protected override void Initialize() {}

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
            Owner.Game.Add(new Worker(Owner));
            numWorkersActive++;
        }
    }
}
