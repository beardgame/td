using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Tiles;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Workers;

interface IWorkerTask : IIdable<IWorkerTask>
{
    string Name { get; }
    IEnumerable<Tile> Tiles { get; }
    double PercentCompleted { get; }
    bool CanAbort { get; }
    bool Finished { get; }

    void Progress(TimeSpan elapsedTime, IWorkerParameters workerParameters);
}
