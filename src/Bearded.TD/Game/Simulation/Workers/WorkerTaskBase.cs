using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Workers;

abstract class WorkerTaskBase : IWorkerTask
{
    private readonly ResourceConsumer resourceConsumer;
    private bool started;

    public Id<IWorkerTask> Id { get; }
    public abstract string Name { get; }
    public IEnumerable<Tile> Tiles { get; }
    public double PercentCompleted => resourceConsumer.PercentageDone;
    public ResourceAmount ResourcesConsumed => resourceConsumer.ResourcesClaimed;
    public bool CanAbort => !started;
    public bool Finished { get; private set; }

    protected abstract bool IsCompleted { get; }
    protected abstract bool IsCancelled { get; }

    protected WorkerTaskBase(Id<IWorkerTask> id,
        IEnumerable<Tile> tiles, GameState gameState, FactionResources.IResourceReservation resourceReservation)
    {
        Id = id;
        Tiles = tiles;
        // The resources per second gets overwritten each frame to account for worker speed changes.
        resourceConsumer = new ResourceConsumer(gameState, resourceReservation, 0.ResourcesPerSecond());
    }

    public void Progress(TimeSpan elapsedTime, IWorkerParameters workerParameters)
    {
        if (IsCompleted)
        {
            complete();
            return;
        }
        if (IsCancelled)
        {
            Abort();
            return;
        }

        if (workerParameters.BuildingSpeed != resourceConsumer.ConsumptionRate)
        {
            resourceConsumer.UpdateConsumptionRate(workerParameters.BuildingSpeed);
        }

        resourceConsumer.PrepareIfNeeded();
        if (!resourceConsumer.CanConsume)
        {
            return;
        }

        if (!started)
        {
            started = true;
            Start();
        }

        resourceConsumer.Update();
        if (!IsCompleted)
        {
            UpdateToMatch();
        }
        if (resourceConsumer.IsDone)
        {
            Complete();
        }
    }

    private void complete()
    {
        resourceConsumer.CompleteIfNeeded();
        Finished = true;
    }

    protected void Abort()
    {
        resourceConsumer.Abort();
        Finished = true;
    }

    protected abstract void Start();
    protected abstract void Complete();
    protected abstract void UpdateToMatch();
}
