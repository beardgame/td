using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Workers;

[FactionBehavior("workers")]
sealed class WorkerTaskManager : FactionBehavior
{
    private readonly List<IWorkerComponent> allWorkers = new();
    private readonly Queue<IWorkerComponent> idleWorkers = new();
    private readonly List<IWorkerTask> tasks = new();
    private readonly IdDictionary<IWorkerTask> tasksById = new();
    private readonly Dictionary<IWorkerTask, IWorkerComponent> workerAssignments = new();

    private int numWorkers => allWorkers.Count;

    public Color WorkerColor => Owner.Color;

    protected override void Execute() {}

    public IWorkerTask TaskFor(Id<IWorkerTask> id) => tasksById[id];

    public void RegisterWorker(IWorkerComponent worker)
    {
        allWorkers.Add(worker);
    }

    public void UnregisterWorker(IWorkerComponent worker)
    {
        allWorkers.Remove(worker);
    }

    public void RegisterTask(IWorkerTask task)
    {
        tasks.Add(task);
        tasksById.Add(task);
        tryAssignTaskToFirstIdleWorker(task);
    }

    public void ReturnTask(IWorkerTask task)
    {
        Argument.Satisfies(!task.Finished);
        workerAssignments.Remove(task);
        tryAssignTaskToFirstIdleWorker(task);
    }

    public void RequestTask(IWorkerComponent worker)
    {
        Argument.Satisfies(() => allWorkers.Contains(worker));
        tasks.Where(isUnassignedTask).MaybeFirst().Match(
            task => assignTask(worker, task),
            () => idleWorkers.Enqueue(worker));
    }

    private void tryAssignTaskToFirstIdleWorker(IWorkerTask task)
    {
        if (idleWorkers.Count > 0)
        {
            assignTask(idleWorkers.Dequeue(), task);
        }
    }

    private void assignTask(IWorkerComponent worker, IWorkerTask task)
    {
        worker.AssignTask(task);
        workerAssignments.Add(task, worker);
    }

    private void suspendTask(IWorkerTask task)
    {
        if (!workerAssignments.TryGetValue(task, out var worker))
            throw new InvalidOperationException("Cannot suspend a task that is not currently assigned.");

        workerAssignments.Remove(task);
        // Note: the worker will immediately look for a new task. That task could be the same task, which is why we
        // delete the assignment first.
        worker.SuspendCurrentTask();
    }

    // TODO: delete
    public void AbortTask(IWorkerTask task)
    {
        var deletedFromList = tasks.Remove(task);
        tasksById.Remove(task);
        State.Satisfies(deletedFromList);

        if (workerAssignments.TryGetValue(task, out var worker))
        {
            workerAssignments.Remove(task);
            // Note: the worker will immediately look for a new task. That task could be the same task, which is why
            // we delete the task and assignment first.
            worker.SuspendCurrentTask();
        }
    }

    public void FinishTask(IWorkerTask task)
    {
        Argument.Satisfies(workerAssignments.ContainsKey(task));
        workerAssignments.Remove(task);
        var deletedFromList = tasks.Remove(task);
        tasksById.Remove(task);
        State.Satisfies(deletedFromList);
    }

    public void BumpTaskToTop(IWorkerTask task)
    {
        tasks.Remove(task);
        tasks.Insert(0, task);

        // Make sure the bumped task is picked up if possible.
        if (!workerAssignments.ContainsKey(task) && numWorkers > 0)
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
            suspendTask(lastAssignedTask);
        }
    }

    private bool isUnassignedTask(IWorkerTask task) => !workerAssignments.ContainsKey(task);
}
