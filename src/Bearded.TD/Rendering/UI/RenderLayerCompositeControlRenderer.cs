﻿using Bearded.TD.UI.Layers;
using Bearded.UI.Rendering;

namespace Bearded.TD.Rendering.UI;

sealed class RenderLayerCompositeControlRenderer : IRenderer<RenderLayerCompositeControl>
{
    private readonly FrameCompositor compositor;

    public RenderLayerCompositeControlRenderer(FrameCompositor compositor)
    {
        this.compositor = compositor;
    }

    public void Render(RenderLayerCompositeControl control)
    {
        control.UpdateViewport(compositor.ViewPort);
        compositor.RenderLayer(control);
    }
}