using System;
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

        public IndexedSurface<PrimitiveVertexData> ConsoleBackground { get; }
        public IndexedSurface<UVColorVertexData> ConsoleFontSurface { get; }
        public Font ConsoleFont { get; }

        public SurfaceManager()
        {
            Shaders.Add(
                ShaderFileLoader.CreateDefault(asset("shaders/")).Load(".")
            );
            new[]
            {
                "geometry", "uvcolor"
            }.ForEach(name => Shaders.MakeShaderProgram(name));

            ConsoleBackground = new IndexedSurface<PrimitiveVertexData>()
                .WithShader(Shaders["geometry"])
                .AndSettings(ViewMatrix, ProjectionMatrix);

            ConsoleFont = Font.FromJsonFile(font("inconsolata.json"));
            ConsoleFontSurface = new IndexedSurface<UVColorVertexData>()
                .WithShader(Shaders["uvcolor"])
                .AndSettings(
                    ViewMatrix, ProjectionMatrix,
                    new TextureUniform("diffuse", new Texture(font("inconsolata.png"), preMultiplyAlpha:true))
                );

        }

        public void InjectDeferredBuffer(Texture buffer)
        {
            var uniform = new TextureUniform("geometry", buffer);

            // TODO: add uniform to light surfaces
        }

        private static string asset(string path) => "assets/" + path;
        private static string font(string path) => asset("font/" + path);

    }

    static class SurfaceExtensions
    {
        public struct SurfaceWrapper<T>
            where T : Surface
        {
            private readonly T surface;

            public SurfaceWrapper(T surface)
            {
                this.surface = surface;
            }

            public T AndSettings(params SurfaceSetting[] settings)
            {
                surface.AddSettings(settings);
                return surface;
            }
        }


        public static SurfaceWrapper<T> WithShader<T>(this T surface, ISurfaceShader shader)
            where T : Surface
        {
            if (shader == null)
                throw new Exception("Shader not found");

            shader.UseOnSurface(surface);
            return new SurfaceWrapper<T>(surface);
        }
    }
}