using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class ProgressTracker
{
    public interface IProgressSubject
    {
        void SendSyncStart();
        void SendSyncComplete();

        void OnStart();
        void OnProgressSet(double percentage);
        void OnComplete();
        void OnCancel();
    }

    private enum ProgressStage : byte
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }

    private ProgressStage progressStage = ProgressStage.NotStarted;
    private ProgressStage syncedProgressStage = ProgressStage.NotStarted;

    public bool IsCompleted => progressStage == ProgressStage.Completed;
    public bool IsCancelled => progressStage == ProgressStage.Cancelled;

    private readonly IProgressSubject subject;

    public ProgressTracker(IProgressSubject subject)
    {
        this.subject = subject;
    }

    public void Start()
    {
        // Avoid duplicate requests.
        if (progressStage > ProgressStage.NotStarted)
        {
            return;
        }

        progressStage = ProgressStage.InProgress;
        subject.SendSyncStart();
    }

    public void SyncStart()
    {
        State.Satisfies(syncedProgressStage == ProgressStage.NotStarted);

        // Catch up the in-memory stage.
        if (progressStage < ProgressStage.InProgress)
        {
            progressStage = ProgressStage.InProgress;
        }

        syncedProgressStage = ProgressStage.InProgress;
        subject.OnStart();
    }

    public void SetProgress(double progress)
    {
        Argument.Satisfies(progress is >= 0 and <= 1);
        State.Satisfies(progressStage == ProgressStage.InProgress);

        // Progress is not synced, so we call the callback directly instead of from the sync command.
        // However, to ensure we don't send progress updates when we haven't called OnStart yet or already called
        // OnComplete or OnCancel, we ignore progress updates if the synced state is not in progress.
        if (syncedProgressStage == ProgressStage.InProgress)
        {
            subject.OnProgressSet(progress);
        }
    }

    public void Complete()
    {
        // Avoid duplicate sync requests.
        if (progressStage > ProgressStage.InProgress)
        {
            return;
        }

        progressStage = ProgressStage.Completed;
        subject.SendSyncComplete();
    }

    public void SyncComplete()
    {
        State.Satisfies(syncedProgressStage == ProgressStage.InProgress);

        // Catch up the in-memory stage.
        if (progressStage < ProgressStage.Completed)
        {
            progressStage = ProgressStage.Completed;
        }
        subject.OnProgressSet(1);

        syncedProgressStage = ProgressStage.Completed;
        subject.OnComplete();
    }

    public void Cancel()
    {
        SetCancelled();
        subject.OnCancel();
    }

    public void SetCancelled()
    {
        progressStage = ProgressStage.Cancelled;
    }
}
