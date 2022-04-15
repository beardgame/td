using System.Collections.Generic;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Workers;

abstract class WorkerState
{
    public static WorkerState Idle(WorkerTaskManager taskManager, IWorkerComponent worker) =>
        new IdleWorkerState(taskManager, worker);
    public static WorkerState ExecuteTask(WorkerTaskManager taskManager, IWorkerComponent worker, IWorkerTask task) =>
        new ExecutingWorkerState(taskManager, worker, task);

    public event GenericEventHandler<WorkerState>? StateChanged;
    public event GenericEventHandler<IEnumerable<Tile>>? TaskTilesChanged;

    private WorkerTaskManager taskManager { get; }
    private IWorkerComponent worker { get; }

    private WorkerState(WorkerTaskManager taskManager, IWorkerComponent worker)
    {
        this.taskManager = taskManager;
        this.worker = worker;
    }

    public abstract void Update(TimeSpan elapsedTime);

    public abstract void Start();
    public abstract void Stop();

    private void moveToState(WorkerState newState) => StateChanged?.Invoke(newState);
    private void setTaskTiles(IEnumerable<Tile> taskTiles) => TaskTilesChanged?.Invoke(taskTiles);

    private sealed class IdleWorkerState : WorkerState
    {
        public IdleWorkerState(WorkerTaskManager taskManager, IWorkerComponent worker) : base(taskManager, worker) { }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Start()
        {
            taskManager.RequestTask(worker);
            setTaskTiles(worker.CurrentTile.Yield());
        }

        public override void Stop() {}
    }

    private sealed class ExecutingWorkerState : WorkerState
    {
        private readonly IWorkerTask task;

        public ExecutingWorkerState(WorkerTaskManager taskManager, IWorkerComponent worker, IWorkerTask task)
            : base(taskManager, worker)
        {
            this.task = task;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (!task.Finished && worker.CurrentTile.NeighboursToTiles(task.Tiles))
            {
                task.Progress(elapsedTime, worker.Parameters);
            }
            if (task.Finished)
            {
                taskManager.FinishTask(task);
                moveToState(Idle(taskManager, worker));
            }
        }

        public override void Start()
        {
            setTaskTiles(task.Tiles);
        }

        public override void Stop()
        {
            if (!task.Finished)
            {
                taskManager.ReturnTask(task);
            }
        }
    }
}
