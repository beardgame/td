using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IIncompleteWork
{
    bool IsCompleted { get; }
    bool IsCancelled { get; }
    ResourceAmount ResourcesInvestedSoFar { get; }
    void StartWork();
    void SetWorkProgress(double percentage, ResourceAmount resourcesInvestedSoFar);
    void CompleteWork();
}
