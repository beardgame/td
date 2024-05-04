using System;
using Bearded.Graphics;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Text;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.Rendering.Vertices;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering;

sealed class CoreRenderers
{
    private readonly CoreShaders shaders;
    private readonly CoreRenderSettings settings;

    public ExpandingIndexedTrianglesMeshBuilder<ColorVertexData> Primitives { get; } = new();
    public IRenderer PrimitivesRenderer { get; }

    public ExpandingIndexedTrianglesMeshBuilder<ColorVertexData> ConsoleBackground { get; } = new();
    public IRenderer ConsoleBackgroundRenderer { get; }

    public IDrawableRenderers DrawableRenderers { get; }
    public TextDrawerWithDefaults<Color> InGameFont { get; private set; } = null!;

    public IndexedTrianglesMeshBuilder<VoidVertex> IntermediateLayerBlur { get; } = new();
    public IRenderer IntermediateLayerBlurRenderer { get; private set; } = null!;

    public GradientBuffer Gradients { get; } = new();
    public ComponentBuffer ShapeComponents { get; } = new();

    public CoreRenderers(CoreShaders shaders, CoreRenderSettings settings, IDrawableRenderers renderers)
    {
        this.shaders = shaders;
        this.settings = settings;
        DrawableRenderers = renderers;

        var primitiveShader = shaders.GetShaderProgram("geometry");

        PrimitivesRenderer = BatchedRenderer.From(
            Primitives.ToRenderable(), settings.ViewMatrix, settings.ProjectionMatrix);
        primitiveShader.UseOnRenderer(PrimitivesRenderer);

        ConsoleBackgroundRenderer = BatchedRenderer.From(
            ConsoleBackground.ToRenderable(), settings.ViewMatrix, settings.ProjectionMatrix);
        primitiveShader.UseOnRenderer(ConsoleBackgroundRenderer);
    }

    public void SetLayerAccumulationTexture(PipelineTexture texture)
    {
        if (IntermediateLayerBlurRenderer != null)
            throw new InvalidOperationException("Layer accumulation texture already set");

        IntermediateLayerBlurRenderer = Renderer.From(
            IntermediateLayerBlur.ToRenderable(),
            settings.ViewMatrix,
            settings.ProjectionMatrix,
            new TextureUniform("inputTexture", TextureUnit.Texture0, texture.Texture)
        );
        shaders.GetShaderProgram("intermediateLayerBlur").UseOnRenderer(IntermediateLayerBlurRenderer);
    }

    public void SetInGameFont(TextDrawerWithDefaults<Color> font)
    {
        InGameFont = font;
    }

    public void FlushShapes()
    {
        Gradients.Flush();
        ShapeComponents.Flush();
    }

    public void ClearAll()
    {
        Primitives.Clear();
        ConsoleBackground.Clear();
        DrawableRenderers.ClearAll();
        Gradients.Clear();
        ShapeComponents.Clear();
        IntermediateLayerBlur.Clear();
    }
}
