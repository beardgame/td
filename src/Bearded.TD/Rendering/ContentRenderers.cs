using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Rendering.Deferred.Level;

namespace Bearded.TD.Rendering;

// TODO: refactor into more semantic interface for deferred renderer
// don't expose actual renderers
sealed class ContentRenderers : IDisposable
{
    private readonly IDrawableRenderers drawableRenderers;
    public LevelRenderer LevelRenderer { get; }
    public ImmutableArray<FluidGeometry> FluidGeometries { get; }

    public ContentRenderers(
        LevelRenderer levelRenderer,
        IEnumerable<FluidGeometry> fluidGeometries,
        IDrawableRenderers drawableRenderers)
    {
        this.drawableRenderers = drawableRenderers;
        LevelRenderer = levelRenderer;
        FluidGeometries = fluidGeometries.ToImmutableArray();
    }

    public void RenderDrawGroup(DrawOrderGroup group)
    {
        drawableRenderers.RenderDrawGroup(group);
    }

    public void Dispose()
    {
        drawableRenderers.DisposeAll();
    }

    public void ClearAll()
    {
        drawableRenderers.ClearAll();
    }
}
