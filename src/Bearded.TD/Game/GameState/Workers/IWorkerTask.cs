using System.Collections.Generic;
using Bearded.TD.Game.GameState.Resources;
using Bearded.TD.Game.Meta;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameState.Workers
{
    interface IWorkerTask : IIdable<IWorkerTask>
    {
        string Name { get; }
        IEnumerable<Tile> Tiles { get; }
        Maybe<ISelectable> Selectable { get; }
        double PercentCompleted { get; }
        bool CanAbort { get; }
        bool Finished { get; }

        void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, ResourceRate ratePerS);
        void OnAbort();
    }
}
