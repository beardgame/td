using Bearded.TD.UI.Layers;
using Bearded.UI.Rendering;

namespace Bearded.TD.Rendering.UI
{
    class RenderLayerCompositeControlRenderer : IRenderer<RenderLayerCompositeControl>
    {
        private readonly FrameCompositor compositor;

        public RenderLayerCompositeControlRenderer(FrameCompositor compositor)
        {
            this.compositor = compositor;
        }

        public void Render(RenderLayerCompositeControl control)
        {
            compositor.RenderLayer(control);
        }
    }
}
