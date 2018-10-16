using System.Collections.Generic;

namespace Bearded.TD.Game.Workers
{
    sealed class WorkerManager
    {
        private readonly Queue<Worker> idleWorkers = new Queue<Worker>();
        private readonly Queue<WorkerTask> tasks = new Queue<WorkerTask>();

        public void RegisterWorker(Worker worker) { }
        public void UnregisterWorker(Worker worker) { }

        public void RegisterIdleWorker(Worker worker)
        {
            if (tasks.Count > 0)
                worker.AssignTask(tasks.Dequeue());
            else
                idleWorkers.Enqueue(worker);
        }

        public void RegisterTask(WorkerTask task)
        {
            if (idleWorkers.Count > 0)
                idleWorkers.Dequeue().AssignTask(task);
            else
                tasks.Enqueue(task);
        }
    }
}
