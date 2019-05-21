using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Workers
{
    sealed class WorkerManager
    {
        private readonly WorkerNetwork network;
        private readonly Queue<Worker> idleWorkers = new Queue<Worker>();
        private readonly List<WorkerTask> tasks = new List<WorkerTask>();
        private readonly Dictionary<WorkerTask, Worker> workerAssignments = new Dictionary<WorkerTask, Worker>();

        public WorkerManager(WorkerNetwork network)
        {
            this.network = network;
            network.NetworkChanged += onNetworkChanged;
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

        public void RegisterWorker(Worker worker) { }
        public void UnregisterWorker(Worker worker) { }

        public void RegisterTask(WorkerTask task)
        {
            tasks.Add(task);
            tryAssignTaskToFirstIdleWorker(task);
        }

        public void ReturnTask(WorkerTask task)
        {
            workerAssignments.Remove(task);
            tryAssignTaskToFirstIdleWorker(task);
        }

        public void RequestTask(Worker worker)
        {
            tasks.Where(isUnassignedTaskInAntennaRange).FirstMaybe().Match(
                task => assignTask(worker, task),
                () => idleWorkers.Enqueue(worker));
        }

        private void tryAssignTaskToFirstIdleWorker(WorkerTask task)
        {
            if (idleWorkers.Count > 0 && isTaskInAntennaRange(task))
            {
                assignTask(idleWorkers.Dequeue(), task);
            }
        }

        private void assignTask(Worker worker, WorkerTask task)
        {
            worker.AssignTask(task);
            workerAssignments.Add(task, worker);
        }

        public void AbortTask(WorkerTask task)
        {
            if (workerAssignments.TryGetValue(task, out var worker))
            {
                worker.AbortCurrentTask();
                workerAssignments.Remove(task);
            }
            var deletedFromList = tasks.Remove(task);
            DebugAssert.State.Satisfies(deletedFromList);
        }

        public void FinishTask(WorkerTask task)
        {
            DebugAssert.Argument.Satisfies(workerAssignments.ContainsKey(task));
            workerAssignments.Remove(task);
            var deletedFromList = tasks.Remove(task);
            DebugAssert.State.Satisfies(deletedFromList);
        }

        private bool isUnassignedTaskInAntennaRange(WorkerTask task) =>
            !workerAssignments.ContainsKey(task) && isTaskInAntennaRange(task);

        private bool isTaskInAntennaRange(WorkerTask task) => task.Tiles.Any(network.IsInRange);
    }
}
