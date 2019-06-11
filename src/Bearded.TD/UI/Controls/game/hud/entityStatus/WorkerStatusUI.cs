using System.Collections.Generic;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Workers;
using Bearded.UI.Navigation;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class WorkerStatusUI : NavigationNode<Faction>
    {
        private GameInstance game;
        public Faction Faction { get; private set; }

        public int NumIdleWorkers => Faction.Workers.NumIdleWorkers;
        public int NumWorkers => Faction.Workers.NumWorkers;
        public IList<IWorkerTask> QueuedTasks => Faction.Workers.QueuedTasks;

        public event VoidEventHandler WorkerValuesUpdated;

        protected override void Initialize(DependencyResolver dependencies, Faction faction)
        {
            game = dependencies.Resolve<GameInstance>();
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

        public void OnTaskCancelClicked(IWorkerTask task)
        {
            game.RequestDispatcher.Dispatch(AbortTask.Request(Faction, task));
        }

        public void OnTaskBumpClicked(IWorkerTask task)
        {
            game.RequestDispatcher.Dispatch(PrioritizeTask.Request(Faction, task));
        }

        public void OnCloseClicked()
        {
            Navigation.Exit();
        }
    }
}
