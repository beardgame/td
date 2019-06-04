using System.Collections.Generic;
using Bearded.TD.Game.Resources;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Workers
{
    abstract class WorkerTask
    {
        public abstract string Name { get; }
        public abstract IEnumerable<Tile> Tiles { get; }
        public abstract bool Finished { get; }

        public abstract void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, double ratePerS);
    }
}
