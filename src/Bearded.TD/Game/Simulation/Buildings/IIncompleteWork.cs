using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IIncompleteWork
{
    bool IsCompleted { get; }
    bool IsCancelled { get; }
    Resource<Scrap> ResourcesInvestedSoFar { get; }
    void StartWork();
    void SetWorkProgress(double percentage, Resource<Scrap> resourcesInvestedSoFar);
    void CompleteWork();
}
