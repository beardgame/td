using System;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Layers;

abstract class DefaultRenderLayerControl : RenderLayerCompositeControl
{
    private const float fovy = MathConstants.PiOver2;
    private const float zNear = .1f;
    private const float zFar = 1024f;

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

    public override Matrix4 ProjectionMatrix
    {
        get
        {
            var yMax = zNear * MathF.Tan(.5f * fovy);
            var yMin = -yMax;
            var xMax = yMax * ViewportSize.AspectRatio;
            var xMin = yMin * ViewportSize.AspectRatio;
            return Matrix4.CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
        }
    }

    protected override void RenderAsLayerBeforeAncestorLayer(IRendererRouter router)
    {
        RenderAsLayer(router);
        SkipNextRender();
    }

    public override RenderOptions RenderOptions { get; } = RenderOptions.Default;
}
