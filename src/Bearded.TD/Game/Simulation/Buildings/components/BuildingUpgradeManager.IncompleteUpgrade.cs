using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed partial class BuildingUpgradeManager
{
    private sealed class IncompleteUpgrade : IncompleteWork, IIncompleteUpgrade
    {
        private readonly BuildingUpgradeManager manager;

        public IPermanentUpgrade Upgrade { get; }
        public double PercentageComplete { get; private set; }

        public IncompleteUpgrade(BuildingUpgradeManager manager, IPermanentUpgrade upgrade)
        {
            this.manager = manager;
            Upgrade = upgrade;
        }

        public override void SendSyncStart()
        {
            manager.sendSyncUpgradeStart(this);
        }

        public override void SendSyncComplete()
        {
            manager.sendSyncUpgradeCompletion(this);
        }

        public override void OnStart() { }

        public override void OnProgressSet(double percentage)
        {
            PercentageComplete = percentage;
        }

        public override void OnComplete()
        {
            manager.onUpgradeCompleted(this);
        }

        public override void OnCancel()
        {
            manager.onUpgradeCancelled(this);
        }

        public void SyncStartUpgrade() => SyncStart();
        public void SyncCompleteUpgrade() => SyncComplete();
    }
}
