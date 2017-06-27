using amulware.Graphics;
using amulware.Graphics.ShaderManagement;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Utilities;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering
{
    class SurfaceManager
    {
        public ShaderManager Shaders { get; } = new ShaderManager();

        public Matrix4Uniform ViewMatrix { get; } = new Matrix4Uniform("view");
        public Matrix4Uniform ProjectionMatrix { get; } = new Matrix4Uniform("projection");

        public IndexedSurface<PrimitiveVertexData> Primitives { get; }
        public IndexedSurface<PrimitiveVertexData> ConsoleBackground { get; }
        public IndexedSurface<UVColorVertexData> ConsoleFontSurface { get; }
        public IndexedSurface<UVColorVertexData> UIFontSurface { get; }
        public Font ConsoleFont { get; }
        public Font UIFont { get; }

        public IndexedSurface<LevelVertex> LevelSurface { get; }
        
        public IndexedSurface<PointLightVertex> PointLights { get; }
        
        public GameSurfaceManager GameSurfaces { get; }

        public SurfaceManager()
        {
            Shaders.Add(
                ShaderFileLoader.CreateDefault(asset("shaders/")).Load(".")
            );
            new[]
            {
                "geometry", "uvcolor",
                "deferred/gSprite",
                "deferred/gLevel",
                "deferred/debug",
                "deferred/compose",
                "deferred/pointlight"
            }.ForEach(name => Shaders.MakeShaderProgram(name));

            Primitives = new IndexedSurface<PrimitiveVertexData>()
                    .WithShader(Shaders["geometry"])
                    .AndSettings(ViewMatrix, ProjectionMatrix);
            ConsoleBackground = new IndexedSurface<PrimitiveVertexData>()
                .WithShader(Shaders["geometry"])
                .AndSettings(ViewMatrix, ProjectionMatrix);

            ConsoleFont = Font.FromJsonFile(font("inconsolata.json"));
            ConsoleFontSurface = new IndexedSurface<UVColorVertexData>()
                .WithShader(Shaders["uvcolor"])
                .AndSettings(
                    ViewMatrix, ProjectionMatrix,
                    new TextureUniform("diffuse", new Texture(font("inconsolata.png"), preMultiplyAlpha: true))
                );

            UIFont = Font.FromJsonFile(font("helveticaneue.json"));
            UIFontSurface = new IndexedSurface<UVColorVertexData>()
                    .WithShader(Shaders["uvcolor"])
                    .AndSettings(
                        ViewMatrix, ProjectionMatrix,
                        new TextureUniform("diffuse", new Texture(font("helveticaneue.png"), preMultiplyAlpha: true))
                    );

            LevelSurface = new IndexedSurface<LevelVertex>()
                .WithShader(Shaders["deferred/gLevel"])
                .AndSettings(ViewMatrix, ProjectionMatrix);
            
            PointLights = new IndexedSurface<PointLightVertex>()
                .WithShader(Shaders["deferred/pointlight"])
                .AndSettings(ViewMatrix, ProjectionMatrix);

            GameSurfaces = new GameSurfaceManager(Shaders, ViewMatrix, ProjectionMatrix);
        }

        public void InjectDeferredBuffer(Texture normalBuffer, Texture depthBuffer)
        {
            var normalUniform = new TextureUniform("normalBuffer", normalBuffer, TextureUnit.Texture0);
            var depthUniform = new TextureUniform("depthBuffer", depthBuffer, TextureUnit.Texture1);
            
            PointLights.AddSettings(normalUniform, depthUniform);
        }

        private static string asset(string path) => "assets/" + path;
        private static string font(string path) => asset("font/" + path);

    }
}
