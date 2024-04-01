using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Shapes;

sealed partial class ShapeDrawer : IDrawable
{
    private readonly Shader shader;
    private readonly ExpandingIndexedTrianglesMeshBuilder<ShapeVertex> meshBuilder = new();
    private readonly IEnumerable<IRenderSetting> settings;
    private readonly IEnumerable<IDisposable> disposables;

    private ShapeDrawer(Shader shader, GradientBuffer gradients, ComponentBuffer components)
    {
        this.shader = shader;
        settings = [
            gradients.TextureUniform("gradientBuffer"),
            components.TextureUniform("componentBuffer", TextureUnit.Texture0 + 1),
        ];
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
