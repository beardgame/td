using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;

namespace Bearded.TD.Mods
{
    sealed class ModLoadingContext
    {
        public Logger Logger { get; }
        public IActionQueue GlActions { get; }

        public ModLoadingContext(Logger logger, IActionQueue glActions)
        {
            Logger = logger;
            GlActions = glActions;
        }
    }
}