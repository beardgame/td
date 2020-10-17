using Bearded.TD.Content;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;

namespace Bearded.TD.Rendering
{
    class RenderContext
    {
        public SurfaceManager Surfaces { get; }
        public GeometryManager Geometries { get; }
        public FrameCompositor Compositor { get; }
        public IGraphicsLoader GraphicsLoader { get; }

        public RenderContext(IActionQueue glActionQueue, Logger logger)
        {
            Surfaces = new SurfaceManager();
            Compositor = new FrameCompositor(logger, Surfaces);
            Geometries = new GeometryManager(Surfaces);
            GraphicsLoader = new GraphicsLoader(this, glActionQueue, logger);
        }

        public void OnResize(ViewportSize viewportSize)
        {
            Compositor.SetViewportSize(viewportSize);
        }
    }
}
