using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed partial class BuildingUpgradeManager
{
    private sealed class IncompleteUpgrade : IncompleteWork, IIncompleteUpgrade
    {
        private readonly BuildingUpgradeManager manager;
        private IBreakageReceipt? breakageReceipt;

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

        public override void OnStart()
        {
            if (breakageReceipt == null
                && manager.Owner.TryGetSingleComponent<IBreakageHandler>(out var breakageHandler))
            {
                breakageReceipt = breakageHandler.BreakObject();
            }
        }

        public override void OnProgressSet(double percentage)
        {
            PercentageComplete = percentage;
        }

        public override void OnComplete()
        {
            manager.onUpgradeCompleted(this);
            breakageReceipt?.Repair();
        }

        public override void OnCancel()
        {
            manager.onUpgradeCancelled(this);
        }

        public void SyncStartUpgrade() => SyncStart();
        public void SyncCompleteUpgrade() => SyncComplete();
    }
}
