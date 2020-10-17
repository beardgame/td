using System.Drawing;
using System.IO;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Rendering;
using amulware.Graphics.RenderSettings;
using amulware.Graphics.ShaderManagement;
using amulware.Graphics.Shapes;
using amulware.Graphics.Text;
using amulware.Graphics.Textures;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Utilities.Collections;
using OpenToolkit.Graphics.OpenGL;
using Font = amulware.Graphics.Text.Font;

namespace Bearded.TD.Rendering
{
    class SurfaceManager
    {
        private static readonly string workingDir = Directory.GetCurrentDirectory() + "/";

        public ShaderManager Shaders { get; } = new ShaderManager();

        public Matrix4Uniform ViewMatrix { get; } = new Matrix4Uniform("view");
        public Matrix4Uniform ViewMatrixLevel { get; } = new Matrix4Uniform("view");
        public Matrix4Uniform ProjectionMatrix { get; } = new Matrix4Uniform("projection");
        public FloatUniform FarPlaneDistance { get; } = new FloatUniform("farPlaneDistance");

        public Vector3Uniform FarPlaneBaseCorner { get; } = new Vector3Uniform("farPlaneBaseCorner");
        public Vector3Uniform FarPlaneUnitX { get; } = new Vector3Uniform("farPlaneUnitX");
        public Vector3Uniform FarPlaneUnitY { get; } = new Vector3Uniform("farPlaneUnitY");
        public Vector3Uniform CameraPosition { get; } = new Vector3Uniform("cameraPosition");

        public FloatUniform Time { get; } = new FloatUniform("time");

        public TextureUniform DepthBuffer { get; set; }

        public ExpandingIndexedTrianglesMeshBuilder<ColorVertexData> Primitives { get; }
            = new ExpandingIndexedTrianglesMeshBuilder<ColorVertexData>();
        public IRenderer PrimitivesRenderer { get; }
        public ExpandingIndexedTrianglesMeshBuilder<ColorVertexData> ConsoleBackground { get; }
            = new ExpandingIndexedTrianglesMeshBuilder<ColorVertexData>();
        public IRenderer ConsoleBackgroundRenderer { get; }
        public ExpandingIndexedTrianglesMeshBuilder<UVColorVertex> ConsoleFontMeshBuilder { get; }
        public IRenderer ConsoleFontRenderer { get; }
        public ExpandingIndexedTrianglesMeshBuilder<UVColorVertex> UIFontMeshBuilder { get; }
        public IRenderer UIFontRenderer { get; }

        public Font ConsoleFont { get; }
        public Font UIFont { get; }

        public ExpandingIndexedTrianglesMeshBuilder<PointLightVertex> PointLights { get; }
            = new ExpandingIndexedTrianglesMeshBuilder<PointLightVertex>();
        public IRenderer PointLightRenderer { get; set; }

        public ExpandingIndexedTrianglesMeshBuilder<SpotlightVertex> Spotlights { get; }
            = new ExpandingIndexedTrianglesMeshBuilder<SpotlightVertex>();
        public IRenderer SpotLightRenderer { get; set; }

        public SurfaceManager()
        {
            var shaderPath = asset("shaders/");

#if DEBUG
            shaderPath = AdjustPathToReloadable(shaderPath);
#endif

            Shaders.AddRange(
                ShaderFileLoader.CreateDefault(shaderPath).Load(".")
            );
            new[]
            {
                "geometry", "uvcolor",
                "deferred/gSprite",
                "deferred/debug",
                "deferred/debugDepth",
                "deferred/compose",
                "deferred/copy",
                "deferred/copyDepth",
                "deferred/pointlight",
                "deferred/spotlight"
            }.ForEach(name => Shaders.RegisterRendererShaderFromAllShadersWithName(name));

            Shaders.TryGetRendererShader("geometry", out var primitiveShader);
            Shaders.TryGetRendererShader("uvcolor", out var uvColorShader);

            PrimitivesRenderer = BatchedRenderer.From(
                Primitives.ToRenderable(), ViewMatrix, ProjectionMatrix);
            primitiveShader!.UseOnRenderer(PrimitivesRenderer);

            ConsoleBackgroundRenderer = BatchedRenderer.From(
                ConsoleBackground.ToRenderable(), ViewMatrix, ProjectionMatrix);
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
                ViewMatrix, ProjectionMatrix, new TextureUniform("diffuse", TextureUnit.Texture0, consoleFontTexture));
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
                ViewMatrix, ProjectionMatrix, new TextureUniform("diffuse", TextureUnit.Texture0, uiFontTexture));
            uvColorShader!.UseOnRenderer(UIFontRenderer);
        }

        public static string AdjustPathToReloadable(string file)
        {
            // point at asset files in the actual repo instead of the binary folder for easy live editing

            // your\td\path
            // \bin\Bearded.TD\Debug\ -> \src\Bearded.TD\
            // assets\file.ext

            var newFile = file
                .Replace("\\", "/")
                .Replace("/bin/Bearded.TD/Debug/", "/src/Bearded.TD/");

            return newFile;
        }

        public void InjectDeferredBuffer(Texture normalBuffer, Texture depthBuffer)
        {
            var normalUniform = new TextureUniform("normalBuffer", TextureUnit.Texture0, normalBuffer);
            DepthBuffer = new TextureUniform("depthBuffer", TextureUnit.Texture1, depthBuffer);

            Shaders.TryGetRendererShader("deferred/pointlight", out var pointLightShader);
            Shaders.TryGetRendererShader("deferred/spotlight", out var spotLightShader);

            PointLightRenderer = BatchedRenderer.From(PointLights.ToRenderable(),
                ViewMatrix, ProjectionMatrix, FarPlaneBaseCorner, FarPlaneUnitX, FarPlaneUnitY, CameraPosition,
                normalUniform, DepthBuffer);
            pointLightShader!.UseOnRenderer(PointLightRenderer);

            SpotLightRenderer = BatchedRenderer.From(Spotlights.ToRenderable(),
                ViewMatrix, ProjectionMatrix, FarPlaneBaseCorner, FarPlaneUnitX, FarPlaneUnitY, CameraPosition,
                normalUniform, DepthBuffer);
            spotLightShader!.UseOnRenderer(SpotLightRenderer);
        }

        private static string asset(string path) => workingDir + "assets/" + path;
        private static string font(string path) => asset("font/" + path);

    }
}
