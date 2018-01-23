using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Resources
{
    abstract class WorkerState
    {
        public static WorkerState Idle(WorkerManager manager, Worker worker) => new IdleWorkerState(manager, worker);
        public static WorkerState ExecuteTask(WorkerManager manager, Worker worker, WorkerTask task)
            => new ExecutingWorkerState(manager, worker, task);

        public event GenericEventHandler<WorkerState> StateChanged;
        public event GenericEventHandler<Position2> GoalPositionChanged; 

        private WorkerManager manager { get; }
        private Worker worker { get; }

        private WorkerState(WorkerManager manager, Worker worker)
        {
            this.manager = manager;
            this.worker = worker;
        }

        public abstract void Update(TimeSpan elapsedTime);

        public abstract void Start();

        private void moveToState(WorkerState newState) => StateChanged?.Invoke(newState);
        private void setGoalPosition(Position2 goalPos) => GoalPositionChanged?.Invoke(goalPos);

        private class IdleWorkerState : WorkerState
        {
            public IdleWorkerState(WorkerManager manager, Worker worker) : base(manager, worker) { }

            public override void Update(TimeSpan elapsedTime) { }

            public override void Start()
            {
                manager.RegisterIdleWorker(worker);
                // TODO: move back to base
            }
        }

        private class ExecutingWorkerState : WorkerState
        {
            private readonly WorkerTask task;

            public ExecutingWorkerState(WorkerManager manager, Worker worker, WorkerTask task)
                : base(manager, worker)
            {
                this.task = task;
            }

            public override void Update(TimeSpan elapsedTime)
            {
                if (!task.Finished && (worker.Position - task.Position).LengthSquared <= worker.WorkRadiusSquared)
                {
                    task.Progress(elapsedTime, worker.Faction.Resources, worker.WorkerSpeed);
                }
                if (task.Finished)
                {
                    moveToState(Idle(manager, worker));
                }
            }

            public override void Start()
            {
                setGoalPosition(task.Position);
            }
        }
    }
}
