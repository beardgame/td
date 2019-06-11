using System.Collections.Generic;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Workers;
using Bearded.UI.Navigation;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class WorkerStatusUI : NavigationNode<Faction>
    {
        public Faction Faction { get; private set; }

        public int NumIdleWorkers => Faction.Workers.NumIdleWorkers;
        public int NumWorkers => Faction.Workers.NumWorkers;
        public IList<WorkerTask> QueuedTasks => Faction.Workers.QueuedTasks;

        public event VoidEventHandler WorkerValuesUpdated;

        protected override void Initialize(DependencyResolver dependencies, Faction faction)
        {
            Faction = faction;

            Faction.Workers.WorkersUpdated += fireWorkerValuesUpdated;
            Faction.Workers.TasksUpdated += fireWorkerValuesUpdated;
        }

        public override void Terminate()
        {
            Faction.Workers.WorkersUpdated -= fireWorkerValuesUpdated;
            Faction.Workers.TasksUpdated -= fireWorkerValuesUpdated;

            base.Terminate();
        }

        private void fireWorkerValuesUpdated()
        {
            WorkerValuesUpdated?.Invoke();
        }

        public void OnCloseClicked()
        {
            Navigation.Exit();
        }
    }
}
