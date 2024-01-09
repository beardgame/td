using Bearded.TD.Content;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;

namespace Bearded.TD.Rendering;

sealed class RenderContext
{
    public CoreShaders Shaders { get; }
    public IGraphicsLoader GraphicsLoader { get; }

    public CoreRenderers Renderers { get; }
    public DeferredRenderer DeferredRenderer { get; }
    public CoreRenderSettings Settings { get; }
    public CoreDrawers Drawers { get; }
    public FrameCompositor Compositor { get; }

    public RenderContext(IActionQueue glActionQueue, Logger logger, IDrawableRenderers drawableRenderers, CoreRenderSettings renderSettings)
    {
        Shaders = new CoreShaders();
        GraphicsLoader = new GraphicsLoader(Shaders.ShaderManager, glActionQueue, logger);

        Settings = renderSettings;
        Renderers = new CoreRenderers(Shaders, Settings, drawableRenderers);
        DeferredRenderer = new DeferredRenderer(Settings, Shaders);
        Compositor = new FrameCompositor(logger, Settings, Shaders, Renderers, DeferredRenderer);
        Drawers = new CoreDrawers(Renderers, DeferredRenderer);
    }

    public void OnResize(ViewportSize viewportSize)
    {
        Compositor.SetViewportSize(viewportSize);
    }
}
