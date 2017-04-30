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
        private readonly Texture accumBuffer = createTexture(); // rgba
        private readonly RenderTarget gTarget = new RenderTarget();
        private readonly RenderTarget accumTarget = new RenderTarget();

        public DeferredRenderer(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;

            gTarget.Attach(FramebufferAttachment.ColorAttachment0, diffuseBuffer);
            gTarget.Attach(FramebufferAttachment.ColorAttachment1, normalHeightBuffer);

            accumTarget.Attach(FramebufferAttachment.ColorAttachment0, accumBuffer);
        }

        private static Texture createTexture()
        {
            var texture = new Texture(1, 1);
            texture.SetParameters(TextureMinFilter.Linear, TextureMagFilter.Linear,
                TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            return texture;
        }

        public void Render(RenderTarget target = null)
        {
            // fill vertex buffers (maybe outside this?)

            resizeIfNeeded();

            renderTo(gTarget);

            clearWithColor();

            // render geometries

            renderTo(accumTarget);

            clearWithColor();

            // render lights to accum buffer (from normal+height only)

            renderTo(target);

            // multiply accum buffer with diffuse
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
            if (!needsResize)
                return;

            resize();

            needsResize = false;
        }

        private void resize()
        {
            diffuseBuffer.Resize(viewport.Width, viewport.Height, PixelInternalFormat.Rgba);
            normalHeightBuffer.Resize(viewport.Width, viewport.Height, PixelInternalFormat.Rgba16f);
            accumBuffer.Resize(viewport.Width, viewport.Height, PixelInternalFormat.Rgba16f);
        }
    }
}