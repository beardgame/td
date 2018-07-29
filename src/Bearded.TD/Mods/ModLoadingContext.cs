using Bearded.Utilities.IO;

namespace Bearded.TD.Mods
{
    sealed class ModLoadingContext
    {
        public Logger Logger { get; }
        public IGraphicsLoader GraphicsLoader { get; }

        public ModLoadingContext(Logger logger, IGraphicsLoader graphicsLoader)
        {
            Logger = logger;
            GraphicsLoader = graphicsLoader;
        }
    }
}