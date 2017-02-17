using System;
using amulware.Graphics;
using amulware.Graphics.ShaderManagement;
using Bearded.TD.Utilities;

namespace Bearded.TD.Rendering
{
    class SurfaceManager
    {
        public Matrix4Uniform ViewMatrix = new Matrix4Uniform("view");
        public Matrix4Uniform ProjectionMatrix = new Matrix4Uniform("projection");
        private readonly ShaderManager shaders = new ShaderManager();

        public IndexedSurface<PrimitiveVertexData> ConsoleBackground { get; }
        public IndexedSurface<UVColorVertexData> ConsoleFontSurface { get; }
        public Font ConsoleFont { get; }

        public SurfaceManager()
        {
            shaders.Add(
                ShaderFileLoader.CreateDefault(asset("shaders/")).Load(".")
            );
            new[]
            {
                "geometry", "uvcolor"
            }.ForEach(name => shaders.MakeShaderProgram(name));

            ConsoleBackground = new IndexedSurface<PrimitiveVertexData>()
                .WithShader(shaders["geometry"])
                .AndSettings(ViewMatrix, ProjectionMatrix);

            ConsoleFont = Font.FromJsonFile(font("inconsolata.json"));
            ConsoleFontSurface = new IndexedSurface<UVColorVertexData>()
                .WithShader(shaders["uvcolor"])
                .AndSettings(
                    ViewMatrix, ProjectionMatrix,
                    new TextureUniform("diffuse", new Texture(font("inconsolata.png"), preMultiplyAlpha:true))
                );

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