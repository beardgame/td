using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed partial class BuildingUpgradeManager
{
    private sealed class IncompleteUpgrade : IIncompleteUpgrade, ProgressTracker.IProgressSubject
    {
        private readonly BuildingUpgradeManager manager;
        private readonly ProgressTracker progressTracker;

        public IUpgradeBlueprint Upgrade { get; }
        public double PercentageComplete { get; private set; }

        public IncompleteUpgrade(BuildingUpgradeManager manager, IUpgradeBlueprint upgrade)
        {
            this.manager = manager;
            progressTracker = new ProgressTracker(this);
            Upgrade = upgrade;
        }

        public void SendSyncStart()
        {
            manager.sendSyncUpgradeStart(this);
        }

        public void SendSyncComplete()
        {
            manager.sendSyncUpgradeCompletion(this);
        }

        public void OnStart() { }

        public void OnProgressSet(double percentage)
        {
            PercentageComplete = percentage;
        }

        public void OnComplete()
        {
            manager.onUpgradeCompleted(this);
        }

        public void OnCancel()
        {
            manager.onUpgradeCancelled(this);
        }

        public bool IsCompleted => progressTracker.IsCompleted;
        public bool IsCancelled => progressTracker.IsCancelled;
        public void StartUpgrade() => progressTracker.Start();
        public void SetUpgradeProgress(double percentage) => progressTracker.SetProgress(percentage);
        public void CompleteUpgrade() => progressTracker.Complete();
        public void CancelUpgrade() => progressTracker.Cancel();

        public void SyncStartUpgrade() => progressTracker.SyncStart();
        public void SyncCompleteUpgrade() => progressTracker.SyncComplete();
    }
}
