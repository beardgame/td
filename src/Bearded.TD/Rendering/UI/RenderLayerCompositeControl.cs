using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;

namespace Bearded.TD.Rendering.UI
{
    abstract class RenderLayerCompositeControl : CompositeControl, IRenderLayer
    {
        private readonly FrameCompositor compositor;

        private IRendererRouter renderRouter;
        private bool skipNextRender;

        public RenderLayerCompositeControl(FrameCompositor compositor)
        {
            this.compositor = compositor;
        }

        public override void Render(IRendererRouter router)
        {
            if (skipNextRender)
            {
                skipNextRender = false;
                return;
            }

            RenderAsLayer(router);
        }

        public void RenderAsLayer(IRendererRouter router)
        {
            renderDescendendRenderLayerControls(router, this);

            renderRouter = router;
            compositor.RenderLayer(this);
            renderRouter = null;
        }

        private void renderDescendendRenderLayerControls(IRendererRouter router, IControlParent parent)
        {
            foreach (var control in parent.Children)
            {
                switch (control)
                {
                    case RenderLayerCompositeControl renderLayer:
                        renderLayer.RenderAsLayer(router);
                        renderLayer.SkipNextRender();
                        break;
                    case IControlParent controlParent:
                        renderDescendendRenderLayerControls(router, controlParent);
                        break;
                }
            }
        }

        public void SkipNextRender()
        {
            skipNextRender = true;
        }

        public abstract Matrix4 ViewMatrix { get; }
        public abstract Matrix4 ProjectionMatrix { get; }
        public abstract RenderOptions RenderOptions { get; }

        public virtual void Draw()
        {
            base.Render(renderRouter);
        }
    }
}
