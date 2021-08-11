using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Buildings
{
    interface IIncompleteUpgrade
    {
        IUpgradeBlueprint Upgrade { get; }
        bool IsCompleted { get; }
        bool IsCancelled { get; }
        double PercentageComplete { get; }

        void StartUpgrade();
        void SetUpgradeProgress(double percentage);
        void CompleteUpgrade();
        void CancelUpgrade();
    }
}
