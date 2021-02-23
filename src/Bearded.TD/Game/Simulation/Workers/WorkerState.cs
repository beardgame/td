﻿using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Workers
{
    abstract class WorkerState
    {
        public static WorkerState Idle(WorkerManager manager, IWorkerComponent worker) =>
            new IdleWorkerState(manager, worker);
        public static WorkerState ExecuteTask(WorkerManager manager, IWorkerComponent worker, IWorkerTask task) =>
            new ExecutingWorkerState(manager, worker, task);

        public event GenericEventHandler<WorkerState>? StateChanged;
        public event GenericEventHandler<IEnumerable<Tile>>? TaskTilesChanged;

        private WorkerManager manager { get; }
        private IWorkerComponent worker { get; }

        private WorkerState(WorkerManager manager, IWorkerComponent worker)
        {
            this.manager = manager;
            this.worker = worker;
        }

        public abstract void Update(TimeSpan elapsedTime);

        public abstract void Start();
        public abstract void Stop();

        private void moveToState(WorkerState newState) => StateChanged?.Invoke(newState);
        private void setTaskTiles(IEnumerable<Tile> taskTiles) => TaskTilesChanged?.Invoke(taskTiles);

        private sealed class IdleWorkerState : WorkerState
        {
            public IdleWorkerState(WorkerManager manager, IWorkerComponent worker) : base(manager, worker) { }

            public override void Update(TimeSpan elapsedTime) { }

            public override void Start()
            {
                manager.RequestTask(worker);
                setTaskTiles(worker.CurrentTile.Yield());
            }

            public override void Stop() {}
        }

        private sealed class ExecutingWorkerState : WorkerState
        {
            private readonly IWorkerTask task;

            public ExecutingWorkerState(WorkerManager manager, IWorkerComponent worker, IWorkerTask task)
                : base(manager, worker)
            {
                this.task = task;
            }

            public override void Update(TimeSpan elapsedTime)
            {
                if (!task.Finished && worker.CurrentTile.NeighboursToTiles(task.Tiles))
                {
                    task.Progress(elapsedTime);
                }
                if (task.Finished)
                {
                    manager.FinishTask(task);
                    moveToState(Idle(manager, worker));
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
                    manager.ReturnTask(task);
                }
            }
        }
    }
}
