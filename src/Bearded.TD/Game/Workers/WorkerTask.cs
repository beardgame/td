using System.Collections.Generic;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Workers
{
    abstract class WorkerTask
    {
        public abstract IEnumerable<Tile<TileInfo>> Tiles { get; }
        public abstract bool Finished { get; }

        public abstract void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, double ratePerS);
    }
}
