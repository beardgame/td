using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Rendering.Deferred.Level;

namespace Bearded.TD.Rendering;

// TODO: this class is very small, review if we need it?
sealed class ContentRenderers : IDisposable
{
    private readonly ISpriteRenderers spriteRenderers;
    public LevelRenderer LevelRenderer { get; }
    public ImmutableArray<FluidGeometry> FluidGeometries { get; }

    public ContentRenderers(
        LevelRenderer levelRenderer,
        IEnumerable<FluidGeometry> fluidGeometries,
        ISpriteRenderers spriteRenderers)
    {
        this.spriteRenderers = spriteRenderers;
        LevelRenderer = levelRenderer;
        FluidGeometries = fluidGeometries.ToImmutableArray();
    }

    public void RenderDrawGroup(SpriteDrawGroup group)
    {
        spriteRenderers.RenderDrawGroup(group);
    }

    public void Dispose()
    {
        spriteRenderers.Dispose();
    }

    public void ClearAll()
    {
        spriteRenderers.ClearAll();
    }
}
