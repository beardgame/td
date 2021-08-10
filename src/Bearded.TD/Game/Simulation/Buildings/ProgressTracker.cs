using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings
{
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
            DebugAssert.State.Satisfies(syncedProgressStage == ProgressStage.NotStarted);

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
            DebugAssert.State.Satisfies(syncedProgressStage == ProgressStage.NotStarted);

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
            DebugAssert.Argument.Satisfies(progress is >= 0 and <= 1);

            // TODO: why is this synced and not just the normal stage?
            DebugAssert.State.Satisfies(syncedProgressStage == ProgressStage.InProgress);
            subject.OnProgressSet(progress);
        }

        public void Complete()
        {
            DebugAssert.State.Satisfies(syncedProgressStage == ProgressStage.InProgress);

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
            DebugAssert.State.Satisfies(syncedProgressStage == ProgressStage.InProgress);

            // Catch up the in-memory stage.
            if (progressStage < ProgressStage.Completed)
            {
                progressStage = ProgressStage.Completed;
            }

            // TODO: catch up remaining progress

            syncedProgressStage = ProgressStage.Completed;
            subject.OnComplete();
        }

        public void Cancel()
        {
            DebugAssert.State.Satisfies(progressStage == ProgressStage.NotStarted);
            subject.OnCancel();
        }
    }
}
