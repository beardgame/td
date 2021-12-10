using Bearded.UI.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Layers;

class DefaultRenderLayerControl : DefaultProjectionRenderLayerControl
{
    public override Matrix4 ViewMatrix
    {
        get
        {
            var originCenter = new Vector3(
                .5f * ViewportSize.ScaledWidth,
                .5f * ViewportSize.ScaledHeight,
                0);
            var eye = new Vector3(0, 0, -.5f * ViewportSize.ScaledHeight) + originCenter;
            return Matrix4.LookAt(
                eye,
                originCenter,
                -Vector3.UnitY);
        }
    }

    protected override void RenderAsLayerBeforeAncestorLayer(IRendererRouter router)
    {
        RenderAsLayer(router);
        SkipNextRender();
    }

    public override RenderOptions RenderOptions { get; } = RenderOptions.Default;
}