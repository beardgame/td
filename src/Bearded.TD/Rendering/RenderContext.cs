using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;

namespace Bearded.TD.Rendering;

sealed class RenderContext
{
    public CoreShaders Shaders { get; }
    public IGraphicsLoader GraphicsLoader { get; }
    public Blueprints CoreAssets { get; }

    public CoreRenderers Renderers { get; }
    public DeferredRenderer DeferredRenderer { get; }
    public CoreRenderSettings Settings { get; }
    public CoreDrawers Drawers { get; }
    public FrameCompositor Compositor { get; }

    public RenderContext(IActionQueue glActionQueue, Logger logger)
    {
        Shaders = new CoreShaders();
        GraphicsLoader = new GraphicsLoader(Shaders.ShaderManager, glActionQueue, logger);
        CoreAssets = loadCoreMod(new ModLoadingContext(logger, GraphicsLoader, new ModLoadingProfiler()));

        Settings = new CoreRenderSettings();
        Renderers = new CoreRenderers(Shaders, Settings);
        DeferredRenderer = new DeferredRenderer(Settings, Shaders);
        Compositor = new FrameCompositor(logger, Settings, Shaders, Renderers, DeferredRenderer);
        Drawers = new CoreDrawers(Renderers, DeferredRenderer);

    }

    private static Blueprints loadCoreMod(ModLoadingContext context)
    {
        var meta = new ModLister().GetAll().First(m => m.Id == "core-ui");
        var dependencies = new List<Mod>().AsReadOnly();

        var mod = ModLoader.Load(context, meta, dependencies).GetAwaiter().GetResult();

        return mod.Blueprints;
    }

    public void OnResize(ViewportSize viewportSize)
    {
        Compositor.SetViewportSize(viewportSize);
    }
}
