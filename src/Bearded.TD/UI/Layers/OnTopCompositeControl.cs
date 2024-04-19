using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Layers;

class OnTopCompositeControl(string debugName) : DefaultRenderLayerControl
{
    public override string DebugName => debugName;

    public new static OnTopCompositeControl CreateClickThrough()
    {
        throw new System.NotImplementedException("Use overload with debug name.");
    }

    public static OnTopCompositeControl CreateClickThrough(string debugName)
    {
        return new OnTopCompositeControl(debugName)
        {
            IsClickThrough = true,
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
