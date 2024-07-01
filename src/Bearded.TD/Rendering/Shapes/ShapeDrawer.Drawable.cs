using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;

namespace Bearded.TD.Rendering.Shapes;

sealed partial class ShapeDrawer : IDrawable
{
    private readonly ExpandingIndexedTrianglesMeshBuilder<ShapeVertex> meshBuilder = new();
    private readonly ImmutableArray<IRenderSetting> settings;
    private readonly ImmutableArray<IDisposable> disposables;

    private ShapeDrawer(ImmutableArray<IRenderSetting> settings)
    {
        this.settings = settings;
        disposables = [meshBuilder];
    }

    public IRenderer CreateRendererWithSettings(IEnumerable<IRenderSetting> additionalSettings)
    {
        return BatchedRenderer.From(
            meshBuilder.ToRenderable(),
            settings.Concat(additionalSettings)
        );
    }

    public void Clear()
    {
        meshBuilder.Clear();
    }

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}
