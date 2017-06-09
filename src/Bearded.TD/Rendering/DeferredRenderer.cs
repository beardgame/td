using System.Linq;
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
        private readonly Texture normalBuffer = createTexture(); // xyz
        private readonly Texture depthBuffer = createTexture(); // z
        private readonly Texture accumBuffer = createTexture(); // rgb
        private readonly RenderTarget gTarget = new RenderTarget();
        private readonly RenderTarget accumTarget = new RenderTarget();

        private readonly IndexedSurface<UVColorVertexData>[] debugSurfaces;

        private readonly PostProcessSurface compositeSurface;

        public DeferredRenderer(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;

            gTarget.Attach(FramebufferAttachment.ColorAttachment0, diffuseBuffer);
            gTarget.Attach(FramebufferAttachment.ColorAttachment1, normalBuffer);
            gTarget.Attach(FramebufferAttachment.ColorAttachment2, depthBuffer);

            accumTarget.Attach(FramebufferAttachment.ColorAttachment0, accumBuffer);

            compositeSurface = new PostProcessSurface()
                .WithShader(surfaces.Shaders.MakeShaderProgram("deferred/compose"))
                .AndSettings(
                    new TextureUniform("albedoTexture", diffuseBuffer),
                    new TextureUniform("lightTexture", accumBuffer)
                );

            surfaces.Shaders.MakeShaderProgram("deferred/debug");

            debugSurfaces = new[] {diffuseBuffer, normalBuffer, depthBuffer, accumBuffer}
                .Select(createDebugSurface).ToArray();
        }

        private IndexedSurface<UVColorVertexData> createDebugSurface(Texture buffer)
            => new IndexedSurface<UVColorVertexData>()
                .WithShader(surfaces.Shaders["deferred/debug"])
                .AndSettings(new TextureUniform("bufferTexture", buffer));

        public void Render(RenderTarget target = null)
        {
            // TODO: fill vertex buffers (maybe outside this?)

            resizeIfNeeded();

            renderWorldToGBuffers();

            renderLightsToAccumBuffer();

            compositeTo(target);
        }

        public void RenderDebug(RenderTarget target = null)
        {
            var width = 1f / debugSurfaces.Length;
            var v2 = width / viewport.AspectRatio;
            var u = 0f;
            var v = 0f;
            var color = Color.White;

            foreach (var surface in debugSurfaces)
            {
                var u2 = u + width;
                
                surface.AddQuad(
                    new UVColorVertexData(u, v, 1, 0, 0, color),
                    new UVColorVertexData(u2, v, 1, 1, 0, color),
                    new UVColorVertexData(u2, v2, 1, 1, 1, color),
                    new UVColorVertexData(u, v2, 1, 0, 1, color)
                    );
                
                u = u2;
            }
        }

        private void renderWorldToGBuffers()
        {
            renderTo(gTarget);

            clearWithColor();

            // TODO: render geometries to g buffers
        }

        private void renderLightsToAccumBuffer()
        {
            renderTo(accumTarget);

            clearWithColor();

            // TODO: render lights to accum buffer (from normal+depth only)
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
            normalBuffer.Resize(viewport.Width, viewport.Height, PixelInternalFormat.Rgba16f);
            depthBuffer.Resize(viewport.Width, viewport.Height, PixelInternalFormat.R16f);
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
