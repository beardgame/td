using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IIncompleteUpgrade : IIncompleteWork
{
    IPermanentUpgrade Upgrade { get; }
    double PercentageComplete { get; }
}
