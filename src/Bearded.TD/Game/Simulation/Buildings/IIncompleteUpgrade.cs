using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IIncompleteUpgrade
{
    IPermanentUpgrade Upgrade { get; }
    bool IsCompleted { get; }
    bool IsCancelled { get; }
    double PercentageComplete { get; }
    ResourceAmount ResourcesInvestedSoFar { get; }

    void StartUpgrade();
    void SetUpgradeProgress(double percentage, ResourceAmount totalResourcesInvested);
    void CompleteUpgrade();
    void CancelUpgrade();
}
