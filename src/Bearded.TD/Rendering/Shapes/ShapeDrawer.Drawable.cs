using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.UI.Gradients;

namespace Bearded.TD.Rendering.Shapes;

sealed partial class ShapeDrawer : IDrawable
{
    private readonly Shader shader;
    private readonly ExpandingIndexedTrianglesMeshBuilder<ShapeVertex> meshBuilder = new();
    private readonly IEnumerable<IRenderSetting> settings;
    private readonly IEnumerable<IDisposable> disposables;

    private ShapeDrawer(Shader shader, Gradients gradients)
    {
        this.shader = shader;
        settings = [gradients.TextureUniform("gradientBuffer")];
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
