using System.Collections.Generic;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.UI.Controls;
using Bearded.UI.Navigation;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class WorkerStatusUI : NavigationNode<Faction>
    {
        private GameInstance game;
        public Faction Faction { get; private set; }
        public bool CanInteract { get; private set; }

        public int NumIdleWorkers => Faction.Workers.NumIdleWorkers;
        public int NumWorkers => Faction.Workers.NumWorkers;
        public IList<IWorkerTask> QueuedTasks => Faction.Workers.QueuedTasks;

        public event VoidEventHandler? WorkerValuesUpdated;

        protected override void Initialize(DependencyResolver dependencies, Faction faction)
        {
            game = dependencies.Resolve<GameInstance>();
            Faction = faction;
            CanInteract = game.Me.Faction.Workers == Faction.Workers;

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

        public void OnTaskHover(IWorkerTask task)
        {
            task.Selectable.Match(selectable => game.SelectionManager.FocusObject(selectable));
        }

        public void OnTaskHoverLeave(IWorkerTask task)
        {
            task.Selectable.Match(_ => game.SelectionManager.ResetFocus());
        }

        public void OnTaskCancelClicked(IWorkerTask task)
        {
            game.Request(AbortTask.Request(Faction, task));
        }

        public void OnTaskBumpClicked(IWorkerTask task)
        {
            game.Request(PrioritizeTask.Request(Faction, task));
        }

        public void OnCloseClicked(Button.ClickEventArgs _)
        {
            Navigation.Exit();
        }
    }
}
