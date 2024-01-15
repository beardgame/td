using Bearded.Graphics;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.Text;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Rendering;

sealed class CoreRenderers
{
    public ExpandingIndexedTrianglesMeshBuilder<ColorVertexData> Primitives { get; } = new();
    public IRenderer PrimitivesRenderer { get; }

    public ExpandingIndexedTrianglesMeshBuilder<ColorVertexData> ConsoleBackground { get; } = new();
    public IRenderer ConsoleBackgroundRenderer { get; }

    public IDrawableRenderers DrawableRenderers { get; }
    public TextDrawerWithDefaults<Color> InGameConsoleFont { get; private set; } = null!;

    public CoreRenderers(CoreShaders shaders, CoreRenderSettings settings, IDrawableRenderers renderers)
    {
        DrawableRenderers = renderers;

        var primitiveShader = shaders.GetShaderProgram("geometry");

        PrimitivesRenderer = BatchedRenderer.From(
            Primitives.ToRenderable(), settings.ViewMatrix, settings.ProjectionMatrix);
        primitiveShader.UseOnRenderer(PrimitivesRenderer);

        ConsoleBackgroundRenderer = BatchedRenderer.From(
            ConsoleBackground.ToRenderable(), settings.ViewMatrix, settings.ProjectionMatrix);
        primitiveShader.UseOnRenderer(ConsoleBackgroundRenderer);
    }

    public void SetInGameConsoleFont(TextDrawerWithDefaults<Color> font)
    {
        InGameConsoleFont = font;
    }

    public void ClearAll()
    {
        Primitives.Clear();
        ConsoleBackground.Clear();
        DrawableRenderers.ClearAll();
    }
}
