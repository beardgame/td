using Bearded.TD.Content;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Utilities;
using Bearded.Utilities.Threading;

namespace Bearded.TD.Rendering
{
    class RenderContext
    {
        public SurfaceManager Surfaces { get; }
        public GeometryManager Geometries { get; }
        public FrameCompositor Compositor { get; }
        public IGraphicsLoader GraphicsLoader { get; }

        public RenderContext(ManualActionQueue glActionQueue)
        {
            Surfaces = new SurfaceManager();
            Geometries = new GeometryManager(Surfaces);
            Compositor = new FrameCompositor(Surfaces);
            GraphicsLoader = new GraphicsLoader(this, glActionQueue);
        }
        
        public void OnResize(ViewportSize viewportSize)
        {
            Compositor.SetViewportSize(viewportSize);
        }
    }
}
