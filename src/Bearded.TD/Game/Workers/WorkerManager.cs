using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Workers
{
    sealed class WorkerManager
    {
        private readonly WorkerNetwork network;
        private readonly List<Worker> allWorkers = new List<Worker>();
        private readonly Queue<Worker> idleWorkers = new Queue<Worker>();
        private readonly List<IWorkerTask> tasks = new List<IWorkerTask>();
        private readonly IdDictionary<IWorkerTask> tasksById = new IdDictionary<IWorkerTask>();
        private readonly Dictionary<IWorkerTask, Worker> workerAssignments = new Dictionary<IWorkerTask, Worker>();

        public int NumWorkers => allWorkers.Count;
        public int NumIdleWorkers => idleWorkers.Count;
        public IList<IWorkerTask> QueuedTasks { get; }

        // Fires when workers are added OR removed.
        public event VoidEventHandler WorkersUpdated;
        // Fires when the task queue changes in any way.
        public event VoidEventHandler TasksUpdated;

        public WorkerManager(WorkerNetwork network)
        {
            this.network = network;
            network.NetworkChanged += onNetworkChanged;
            QueuedTasks = tasks.AsReadOnly();
        }

        public IWorkerTask TaskFor(Id<IWorkerTask> id) => tasksById[id];

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
            tasksById.Add(task);
            tryAssignTaskToFirstIdleWorker(task);
            TasksUpdated?.Invoke();
        }

        public void ReturnTask(IWorkerTask task)
        {
            DebugAssert.Argument.Satisfies(!task.Finished);
            workerAssignments.Remove(task);
            tryAssignTaskToFirstIdleWorker(task);
        }

        public void RequestTask(Worker worker)
        {
            DebugAssert.Argument.Satisfies(() => allWorkers.Contains(worker));
            tasks.Where(isUnassignedTaskInAntennaRange).MaybeFirst().Match(
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

        public void SuspendTask(IWorkerTask task)
        {
            if (!workerAssignments.TryGetValue(task, out var worker))
                throw new InvalidOperationException("Cannot suspend a task that is not currently assigned.");

            workerAssignments.Remove(task);
            // Note: the worker will immediately look for a new task. That task could be the same task, which is why we
            // delete the assignment first.
            worker.SuspendCurrentTask();
            TasksUpdated?.Invoke();
        }

        public void AbortTask(IWorkerTask task)
        {
            var deletedFromList = tasks.Remove(task);
            tasksById.Remove(task);
            DebugAssert.State.Satisfies(deletedFromList);
            task.OnAbort();

            if (workerAssignments.TryGetValue(task, out var worker))
            {
                workerAssignments.Remove(task);
                // Note: the worker will immediately look for a new task. That task could be the same task, which is why
                // we delete the task and assignment first.
                worker.SuspendCurrentTask();
            }
            TasksUpdated?.Invoke();
        }

        public void FinishTask(IWorkerTask task)
        {
            DebugAssert.Argument.Satisfies(workerAssignments.ContainsKey(task));
            workerAssignments.Remove(task);
            var deletedFromList = tasks.Remove(task);
            tasksById.Remove(task);
            DebugAssert.State.Satisfies(deletedFromList);
            TasksUpdated?.Invoke();
        }

        public void BumpTaskToTop(IWorkerTask task)
        {
            tasks.Remove(task);
            tasks.Insert(0, task);

            TasksUpdated?.Invoke();

            // Make sure the bumped task is picked up if possible.
            if (!workerAssignments.ContainsKey(task) && NumWorkers > 0)
            {
                tickleLatestWorker();
            }
        }

        private void tickleLatestWorker()
        {
            // Look for the latest task with a worker assigned. Force that worker to pick a new task in case there is a
            // task that is now higher on the queue. If there isn't, we expect it to resume the same task straightaway.
            var lastAssignedTask = tasks.Where(workerAssignments.ContainsKey).LastOrDefault();
            if (lastAssignedTask != null)
            {
                SuspendTask(lastAssignedTask);
            }
        }

        private bool isUnassignedTaskInAntennaRange(IWorkerTask task) =>
            !workerAssignments.ContainsKey(task) && isTaskInAntennaRange(task);

        private bool isTaskInAntennaRange(IWorkerTask task) => task.Tiles.Any(network.IsInRange);
    }
}
