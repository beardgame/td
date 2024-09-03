using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Buildings;

abstract class IncompleteWork : IIncompleteWork, ProgressTracker.IProgressSubject
{
    private readonly ProgressTracker progressTracker;

    public bool IsCompleted => progressTracker.IsCompleted;
    public bool IsCancelled => progressTracker.IsCancelled;
    public Resource<Scrap> ResourcesInvestedSoFar { get; private set; }

    protected IncompleteWork()
    {
        progressTracker = new ProgressTracker(this);
    }

    protected void MakeCancelled()
    {
        if (!IsCancelled)
        {
            progressTracker.SetCancelled();
        }
    }

    public void StartWork()
    {
        progressTracker.Start();
    }

    public void SetWorkProgress(double percentage, Resource<Scrap> resourcesInvestedSoFar)
    {
        progressTracker.SetProgress(percentage);
        ResourcesInvestedSoFar = resourcesInvestedSoFar;
    }

    public void CompleteWork()
    {
        progressTracker.Complete();
    }

    protected void SyncStart() => progressTracker.SyncStart();
    protected void SyncComplete() => progressTracker.SyncComplete();

    public abstract void SendSyncStart();
    public abstract void SendSyncComplete();
    public abstract void OnStart();
    public abstract void OnProgressSet(double percentage);
    public abstract void OnComplete();
    public abstract void OnCancel();
}
