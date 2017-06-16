using amulware.Graphics;
using amulware.Graphics.ShaderManagement;
using Bearded.TD.Utilities;

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
        
        public GameSurfaceManager GameSurfaces { get; }

        public SurfaceManager()
        {
            Shaders.Add(
                ShaderFileLoader.CreateDefault(asset("shaders/")).Load(".")
            );
            new[]
            {
                "geometry", "uvcolor",
                "deferred/gSprite", "deferred/debug", "deferred/compose"
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

            GameSurfaces = new GameSurfaceManager(Shaders, ViewMatrix, ProjectionMatrix);
        }

        public void InjectDeferredBuffer(Texture normalBuffer, Texture depthBuffer)
        {
            var normalUniform = new TextureUniform("normalBuffer", normalBuffer);
            var depthUniform = new TextureUniform("depthBuffer", depthBuffer);

            // TODO: add uniform to light surfaces
        }

        private static string asset(string path) => "assets/" + path;
        private static string font(string path) => asset("font/" + path);

    }
}
