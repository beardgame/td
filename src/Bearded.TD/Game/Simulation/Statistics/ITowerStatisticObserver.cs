using Bearded.TD.Game.Simulation.Statistics.Data;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Statistics;

interface ITowerStatisticObserver
{
    event VoidEventHandler StatisticsUpdated;
    event VoidEventHandler Disposed;

    AccumulatedDamage TotalDamage { get; }

    void StopObserving();
}
