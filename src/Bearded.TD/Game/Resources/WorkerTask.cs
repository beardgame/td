using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Resources
{
    abstract class WorkerTask
    {
        public abstract Position2 Position { get; }
        public abstract bool Finished { get; }

        public abstract void Progress(ResourceManager resourceManager, double ratePerS);
    }
}
