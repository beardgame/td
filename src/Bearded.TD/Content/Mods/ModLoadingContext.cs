using Bearded.Utilities.IO;

namespace Bearded.TD.Content.Mods;

sealed class ModLoadingContext
{
    public Logger Logger { get; }
    public IGraphicsLoader GraphicsLoader { get; }
    public ModLoadingProfiler Profiler { get; }

    public ModLoadingContext(Logger logger, IGraphicsLoader graphicsLoader, ModLoadingProfiler profiler)
    {
        Logger = logger;
        GraphicsLoader = graphicsLoader;
        Profiler = profiler;
    }
}