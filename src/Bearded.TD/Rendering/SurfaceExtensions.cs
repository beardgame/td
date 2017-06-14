using System;
using amulware.Graphics;

namespace Bearded.TD.Rendering
{
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