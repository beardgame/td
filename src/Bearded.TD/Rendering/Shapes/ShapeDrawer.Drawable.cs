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
    private readonly ExpandingIndexedTrianglesMeshBuilder<ShapeVertex> mesh = new();
    private readonly IMeshBuilder meshBuilder;
    private readonly ImmutableArray<IRenderSetting> settings;
    private readonly ImmutableArray<IDisposable> disposables;

    private ShapeDrawer(
        Func<IIndexedTrianglesMeshBuilder<ShapeVertex, ushort>, IMeshBuilder>? meshBuilderFactory,
        ImmutableArray<IRenderSetting> settings)
    {
        meshBuilder = meshBuilderFactory?.Invoke(mesh) ?? new DefaultMeshBuilder(mesh);
        this.settings = settings;
        disposables = [mesh];
    }

    public IRenderer CreateRendererWithSettings(IEnumerable<IRenderSetting> additionalSettings)
    {
        return BatchedRenderer.From(
            mesh.ToRenderable(),
            settings.Concat(additionalSettings)
        );
    }

    public void Clear()
    {
        mesh.Clear();
    }

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}
