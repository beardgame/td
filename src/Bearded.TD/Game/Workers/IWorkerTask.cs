using System.Collections.Generic;
using Bearded.TD.Commands;
using Bearded.TD.Game.Resources;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Workers
{
    interface IWorkerTask
    {
        string Name { get; }
        IEnumerable<Tile> Tiles { get; }
        double PercentCompleted { get; }
        bool Finished { get; }

        void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, double ratePerS);
        IRequest<GameInstance> CancelRequest();
    }
}
