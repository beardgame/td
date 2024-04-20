using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Rendering.Shapes;

sealed partial class ShapeDrawer : IDrawable
{
    private readonly Shader shader;
    private readonly ExpandingIndexedTrianglesMeshBuilder<ShapeVertex> meshBuilder = new();
    private readonly ImmutableArray<IRenderSetting> settings;
    private readonly ImmutableArray<IDisposable> disposables;

    private ShapeDrawer(Shader shader, ImmutableArray<IRenderSetting> settings)
    {
        this.shader = shader;
        this.settings = settings;
        disposables = [meshBuilder];
    }

    public IRenderer CreateRendererWithSettings(IEnumerable<IRenderSetting> additionalSettings)
    {
        var renderer = BatchedRenderer.From(
            meshBuilder.ToRenderable(),
            settings.Concat(additionalSettings)
        );

        shader.RendererShader.UseOnRenderer(renderer);

        return renderer;
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
