using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Layers;

class OnTopCompositeControl : DefaultRenderLayerControl
{
    public new static OnTopCompositeControl CreateClickThrough()
    {
        return new OnTopCompositeControl
        {
            IsClickThrough = true
        };
    }

    protected override void RenderAsLayerBeforeAncestorLayer(IRendererRouter router)
    {
        SkipNextRender();
    }

    protected override void RenderAsLayerAfterAncestorLayer(IRendererRouter router)
    {
        RenderAsLayer(router);
    }
}
