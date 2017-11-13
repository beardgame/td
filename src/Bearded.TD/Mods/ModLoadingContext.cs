using Bearded.Utilities.IO;

namespace Bearded.TD.Mods
{
    sealed class ModLoadingContext
    {
        public Logger Logger { get; }

        public ModLoadingContext(Logger logger)
        {
            Logger = logger;
        }

    }
}