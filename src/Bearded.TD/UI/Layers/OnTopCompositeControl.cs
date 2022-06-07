using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Layers;

abstract class OnTopCompositeControl : DefaultRenderLayerControl
{
    protected override void RenderAsLayerBeforeAncestorLayer(IRendererRouter router)
    {
        SkipNextRender();
    }

    protected override void RenderAsLayerAfterAncestorLayer(IRendererRouter router)
    {
        RenderAsLayer(router);
    }
}
