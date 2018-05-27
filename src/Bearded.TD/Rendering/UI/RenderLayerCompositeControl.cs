
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;

namespace Bearded.TD.Rendering.UI
{
    abstract class RenderLayerCompositeControl : CompositeControl, IRenderLayer
    {
        private readonly FrameCompositor compositor;

        private IRendererRouter renderRouter;

        public RenderLayerCompositeControl(FrameCompositor compositor)
        {
            this.compositor = compositor;
        }

        public override void Render(IRendererRouter router)
        {
            renderRouter = router;
            
            compositor.RenderLayer(this);
            
            renderRouter = null;
        }

        abstract public Matrix4 ViewMatrix { get; }
        abstract public Matrix4 ProjectionMatrix { get; }
        abstract public RenderOptions RenderOptions { get; }

        public virtual void Draw() => base.Render(renderRouter);
    }
}
