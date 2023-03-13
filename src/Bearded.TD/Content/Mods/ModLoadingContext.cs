using System.Collections.Generic;
using Bearded.Utilities.IO;

namespace Bearded.TD.Content.Mods;

sealed class ModLoadingContext
{
    private readonly List<ModLoadingError> errors = new();
    public Logger Logger { get; }
    public IGraphicsLoader GraphicsLoader { get; }
    public ModLoadingProfiler Profiler { get; }

    public IReadOnlyCollection<ModLoadingError> Errors { get; }

    public ModLoadingContext(Logger logger, IGraphicsLoader graphicsLoader, ModLoadingProfiler profiler)
    {
        Logger = logger;
        GraphicsLoader = graphicsLoader;
        Profiler = profiler;
        Errors = errors.AsReadOnly();
    }

    public void AddError(ModLoadingError error)
    {
        errors.Add(error);
    }
}
