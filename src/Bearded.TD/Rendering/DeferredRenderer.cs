using amulware.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering
{
    class DeferredRenderer
    {
        private readonly SurfaceManager surfaces;
        private ViewportSize viewport;
        private bool needsResize;

        private readonly Texture diffuseBuffer = createTexture(); // rgba
        private readonly Texture normalHeightBuffer = createTexture(); // xyzh
        private readonly Texture accumBuffer = createTexture(); // rgb
        private readonly RenderTarget gTarget = new RenderTarget();
        private readonly RenderTarget accumTarget = new RenderTarget();

        private readonly PostProcessSurface compositeSurface;

        public DeferredRenderer(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;

            gTarget.Attach(FramebufferAttachment.ColorAttachment0, diffuseBuffer);
            gTarget.Attach(FramebufferAttachment.ColorAttachment1, normalHeightBuffer);

            accumTarget.Attach(FramebufferAttachment.ColorAttachment0, accumBuffer);

            surfaces.Shaders.MakeShaderProgram("" /* TODO */);
            compositeSurface = new PostProcessSurface()
                .WithShader(surfaces.Shaders["" /* TODO */ ])
                .AndSettings(
                    new TextureUniform("diffuse", diffuseBuffer),
                    new TextureUniform("light", accumBuffer)
                );
        }

        public void Render(RenderTarget target = null)
        {
            // TODO: fill vertex buffers (maybe outside this?)

            resizeIfNeeded();

            renderWorldToGBuffers();

            renderLightsToAccumBuffer();

            compositeTo(target);
        }

        private void renderWorldToGBuffers()
        {
            renderTo(gTarget);

            clearWithColor();

            // TODO: render geometries
        }

        private void renderLightsToAccumBuffer()
        {
            renderTo(accumTarget);

            clearWithColor();

            // TODO: render lights to accum buffer (from normal+height only)
        }

        private void compositeTo(RenderTarget target)
        {
            renderTo(target);

            compositeSurface.Render();
        }

        private static void renderTo(RenderTarget target)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, target);
        }

        private static void clearWithColor(float r = 0, float g = 0, float b = 0, float a = 0)
        {
            GL.ClearColor(r, g, b, a);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }


        public void OnResize(ViewportSize viewport)
        {
            if (viewport == this.viewport)
                return;

            this.viewport = viewport;
            needsResize = true;
        }

        private void resizeIfNeeded()
        {
            if (needsResize)
                resize();

            needsResize = false;
        }

        private void resize()
        {
            diffuseBuffer.Resize(viewport.Width, viewport.Height, PixelInternalFormat.Rgba);
            normalHeightBuffer.Resize(viewport.Width, viewport.Height, PixelInternalFormat.Rgba16f);
            accumBuffer.Resize(viewport.Width, viewport.Height, PixelInternalFormat.Rgb16f);
        }

        private static Texture createTexture()
        {
            var texture = new Texture(1, 1);
            texture.SetParameters(TextureMinFilter.Linear, TextureMagFilter.Linear,
                TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            return texture;
        }
    }
}