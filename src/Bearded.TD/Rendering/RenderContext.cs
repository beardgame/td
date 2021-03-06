﻿using Bearded.TD.Content;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;

namespace Bearded.TD.Rendering
{
    sealed class RenderContext
    {
        public CoreShaders Shaders { get; }
        public CoreRenderers Renderers { get; }
        public DeferredRenderer DeferredRenderer { get; }
        public CoreRenderSettings Settings { get; }
        public CoreDrawers Drawers { get; }
        public FrameCompositor Compositor { get; }
        public IGraphicsLoader GraphicsLoader { get; }

        public RenderContext(IActionQueue glActionQueue, Logger logger)
        {
            Shaders = new CoreShaders();
            Settings = new CoreRenderSettings();
            Renderers = new CoreRenderers(Shaders, Settings);
            DeferredRenderer = new DeferredRenderer(Settings, Shaders);
            Compositor = new FrameCompositor(logger, Settings, Shaders, Renderers, DeferredRenderer);
            Drawers = new CoreDrawers(Renderers, DeferredRenderer);
            GraphicsLoader = new GraphicsLoader(this, glActionQueue, logger);
        }

        public void OnResize(ViewportSize viewportSize)
        {
            Compositor.SetViewportSize(viewportSize);
        }
    }
}
