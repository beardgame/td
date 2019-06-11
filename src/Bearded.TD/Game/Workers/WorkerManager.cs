using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game.Workers
{
    sealed class WorkerManager
    {
        private readonly WorkerNetwork network;
        private readonly List<Worker> allWorkers = new List<Worker>();
        private readonly Queue<Worker> idleWorkers = new Queue<Worker>();
        private readonly List<IWorkerTask> tasks = new List<IWorkerTask>();
        private readonly Dictionary<IWorkerTask, Worker> workerAssignments = new Dictionary<IWorkerTask, Worker>();

        public int NumWorkers => allWorkers.Count;
        public int NumIdleWorkers => idleWorkers.Count;
        public IList<IWorkerTask> QueuedTasks { get; }

        // Fires when workers are added OR removed.
        public event VoidEventHandler WorkersUpdated;
        // Fires when tasks are finished OR cancelled.
        public event VoidEventHandler TasksUpdated;

        public WorkerManager(WorkerNetwork network)
        {
            this.network = network;
            network.NetworkChanged += onNetworkChanged;
            QueuedTasks = tasks.AsReadOnly();
        }

        private void onNetworkChanged()
        {
            var tasksOutOfRange = workerAssignments.Keys.Where(task => !task.Tiles.Any(network.IsInRange)).ToList();
            foreach (var task in tasksOutOfRange) AbortTask(task);

            if (idleWorkers.Count == 0) return;
            foreach (var task in tasks.Where(isUnassignedTaskInAntennaRange).Take(idleWorkers.Count))
            {
                assignTask(idleWorkers.Dequeue(), task);
            }
        }

        public void RegisterWorker(Worker worker)
        {
            allWorkers.Add(worker);
            WorkersUpdated?.Invoke();
        }

        public void UnregisterWorker(Worker worker)
        {
            allWorkers.Remove(worker);
            WorkersUpdated?.Invoke();
        }

        public void RegisterTask(IWorkerTask task)
        {
            tasks.Add(task);
            tryAssignTaskToFirstIdleWorker(task);
            TasksUpdated?.Invoke();
        }

        public void ReturnTask(IWorkerTask task)
        {
            workerAssignments.Remove(task);
            tryAssignTaskToFirstIdleWorker(task);
        }

        public void RequestTask(Worker worker)
        {
            DebugAssert.Argument.Satisfies(() => allWorkers.Contains(worker));
            tasks.Where(isUnassignedTaskInAntennaRange).FirstMaybe().Match(
                task => assignTask(worker, task),
                () => idleWorkers.Enqueue(worker));
        }

        private void tryAssignTaskToFirstIdleWorker(IWorkerTask task)
        {
            if (idleWorkers.Count > 0 && isTaskInAntennaRange(task))
            {
                assignTask(idleWorkers.Dequeue(), task);
            }
        }

        private void assignTask(Worker worker, IWorkerTask task)
        {
            worker.AssignTask(task);
            workerAssignments.Add(task, worker);
        }

        public void AbortTask(IWorkerTask task)
        {
            if (workerAssignments.TryGetValue(task, out var worker))
            {
                worker.AbortCurrentTask();
                workerAssignments.Remove(task);
            }
            var deletedFromList = tasks.Remove(task);
            DebugAssert.State.Satisfies(deletedFromList);
            TasksUpdated?.Invoke();
        }

        public void FinishTask(IWorkerTask task)
        {
            DebugAssert.Argument.Satisfies(workerAssignments.ContainsKey(task));
            workerAssignments.Remove(task);
            var deletedFromList = tasks.Remove(task);
            DebugAssert.State.Satisfies(deletedFromList);
            TasksUpdated?.Invoke();
        }

        private bool isUnassignedTaskInAntennaRange(IWorkerTask task) =>
            !workerAssignments.ContainsKey(task) && isTaskInAntennaRange(task);

        private bool isTaskInAntennaRange(IWorkerTask task) => task.Tiles.Any(network.IsInRange);
    }
}
