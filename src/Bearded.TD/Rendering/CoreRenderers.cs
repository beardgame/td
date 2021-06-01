using System.Drawing;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Text;
using Bearded.TD.Rendering.Vertices;
using OpenTK.Graphics.OpenGL;
using Font = Bearded.Graphics.Text.Font;

namespace Bearded.TD.Rendering
{
    sealed class CoreRenderers
    {
        public Font ConsoleFont { get; }
        public Font UIFont { get; }

        public ExpandingIndexedTrianglesMeshBuilder<ColorVertexData> Primitives { get; } = new();
        public IRenderer PrimitivesRenderer { get; }

        public ExpandingIndexedTrianglesMeshBuilder<ColorVertexData> ConsoleBackground { get; } = new();
        public IRenderer ConsoleBackgroundRenderer { get; }

        public ExpandingIndexedTrianglesMeshBuilder<UVColorVertex> ConsoleFontMeshBuilder { get; }
        public IRenderer ConsoleFontRenderer { get; }

        public ExpandingIndexedTrianglesMeshBuilder<UVColorVertex> UIFontMeshBuilder { get; }
        public IRenderer UIFontRenderer { get; }


        public CoreRenderers(CoreShaders shaders, CoreRenderSettings settings)
        {
            var primitiveShader = shaders.GetShaderProgram("geometry");
            var uvColorShader = shaders.GetShaderProgram("uvcolor");

            PrimitivesRenderer = BatchedRenderer.From(
                Primitives.ToRenderable(), settings.ViewMatrix, settings.ProjectionMatrix);
            primitiveShader!.UseOnRenderer(PrimitivesRenderer);

            ConsoleBackgroundRenderer = BatchedRenderer.From(
                ConsoleBackground.ToRenderable(), settings.ViewMatrix, settings.ProjectionMatrix);
            primitiveShader!.UseOnRenderer(ConsoleBackgroundRenderer);

            var (consoleFontTextureData, consoleFont) = // used to be inconsolata
                FontFactory.From(new System.Drawing.Font(FontFamily.GenericMonospace, 32), 2);
            // TODO: premultiply console font texture data!
            var consoleFontTexture = consoleFontTextureData.ToTexture(t =>
            {
                t.SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
                t.GenerateMipmap();
            });

            ConsoleFont = consoleFont;
            ConsoleFontMeshBuilder = new ExpandingIndexedTrianglesMeshBuilder<UVColorVertex>();
            ConsoleFontRenderer = BatchedRenderer.From(ConsoleFontMeshBuilder.ToRenderable(),
                settings.ViewMatrix, settings.ProjectionMatrix, new TextureUniform("diffuse", TextureUnit.Texture0, consoleFontTexture));
            uvColorShader!.UseOnRenderer(ConsoleFontRenderer);

            var (uiFontTextureData, uiFont) = // used to be helveticaneue
                FontFactory.From(new System.Drawing.Font(FontFamily.GenericSansSerif, 32), 2);
            // TODO: premultiply console font texture data!
            var uiFontTexture = uiFontTextureData.ToTexture(t =>
            {
                t.SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
                t.GenerateMipmap();
            });

            UIFont = uiFont;
            UIFontMeshBuilder = new ExpandingIndexedTrianglesMeshBuilder<UVColorVertex>();
            UIFontRenderer = BatchedRenderer.From(UIFontMeshBuilder.ToRenderable(),
                settings.ViewMatrix, settings.ProjectionMatrix, new TextureUniform("diffuse", TextureUnit.Texture0, uiFontTexture));
            uvColorShader!.UseOnRenderer(UIFontRenderer);
        }

        public void ClearAll()
        {
            Primitives.Clear();
            ConsoleBackground.Clear();
            ConsoleFontMeshBuilder.Clear();
            UIFontMeshBuilder.Clear();
        }
    }
}
