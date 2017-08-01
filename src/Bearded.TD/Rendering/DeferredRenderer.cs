using System;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Meta;
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
        private readonly Texture depthMaskBuffer = createDepthTexture();

        private readonly RenderTarget gTarget = new RenderTarget();
        private readonly RenderTarget accumTarget = new RenderTarget();

        private readonly IndexedSurface<UVColorVertexData>[] debugSurfaces;

        private readonly PostProcessSurface compositeSurface;
        private (int width, int height) bufferSize;

        public DeferredRenderer(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;

            gTarget.Attach(FramebufferAttachment.ColorAttachment0, diffuseBuffer);
            gTarget.Attach(FramebufferAttachment.ColorAttachment1, normalBuffer);
            gTarget.Attach(FramebufferAttachment.ColorAttachment2, depthBuffer);
            gTarget.Attach(FramebufferAttachment.DepthAttachment, depthMaskBuffer);
            renderTo(gTarget, (0, 0));
            GL.DrawBuffers(3, new []
            {
                DrawBuffersEnum.ColorAttachment0,
                DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2,
            });
            renderTo(null, (0, 0));

            accumTarget.Attach(FramebufferAttachment.ColorAttachment0, accumBuffer);

            compositeSurface = new PostProcessSurface()
                .WithShader(surfaces.Shaders["deferred/compose"])
                .AndSettings(
                    new TextureUniform("albedoTexture", diffuseBuffer, TextureUnit.Texture0),
                    new TextureUniform("lightTexture", accumBuffer, TextureUnit.Texture1)
                );

            debugSurfaces = new[] {diffuseBuffer, normalBuffer, depthBuffer, accumBuffer}
                .Select(createDebugSurface).ToArray();
            
            surfaces.InjectDeferredBuffer(normalBuffer, depthBuffer);
        }

        private IndexedSurface<UVColorVertexData> createDebugSurface(Texture buffer)
            => new IndexedSurface<UVColorVertexData>()
                .WithShader(surfaces.Shaders["deferred/debug"])
                .AndSettings(new TextureUniform("bufferTexture", buffer));

        public void Render(RenderTarget target = null)
        {
            resizeIfNeeded();

            renderWorldToGBuffers();

            renderLightsToAccumBuffer();

            compositeTo(target);
        }

        public void RenderDebug(RenderTarget target = null)
        {
            var width = 2f / debugSurfaces.Length;
            var height = 2f / debugSurfaces.Length;
            var u = -1f;
            var v = -1f;
            var color = Color.White;

            foreach (var surface in debugSurfaces)
            {
                var u2 = u + width * 0.95f;
                var v2 = v + height * 0.95f;
                surface.AddQuad(
                    new UVColorVertexData(u, v, 0, 0, 0, color),
                    new UVColorVertexData(u2, v, 0, 1, 0, color),
                    new UVColorVertexData(u2, v2, 0, 1, 1, color),
                    new UVColorVertexData(u, v2, 0, 0, 1, color)
                );
                
                u = u + width;
                
                surface.Render();
            }
        }

        private void renderWorldToGBuffers()
        {
            renderTo(gTarget, bufferSize);
            
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            GL.Disable(EnableCap.Blend);

            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            surfaces.LevelSurface.Render();
            
            GL.DepthMask(false);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);

            foreach (var surface in surfaces.GameSurfaces.SurfaceList)
            {
                surface.Render();
            }

            GL.Disable(EnableCap.DepthTest);
        }

        private void renderLightsToAccumBuffer()
        {
            renderTo(accumTarget, bufferSize);

            clearWithColor();

            surfaces.PointLights.Render();
        }

        private void compositeTo(RenderTarget target)
        {
            renderTo(target, (viewport.Width, viewport.Height));

            compositeSurface.Render();
        }

        private static void renderTo(RenderTarget target, (int width, int height) viewport)
        {
            GL.Viewport(0, 0, viewport.width, viewport.height);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, target);
        }

        private static void clearWithColor(float r = 0, float g = 0, float b = 0, float a = 0)
        {
            GL.ClearColor(r, g, b, a);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }


        public void OnResize(ViewportSize viewport)
        {
            var bufferSizeFactor = 1 / UserSettings.Instance.Graphics.UpSample;
            
            var w = (int) (viewport.Width * bufferSizeFactor);
            var h = (int) (viewport.Height * bufferSizeFactor);

            if (viewport == this.viewport && (w, h).Equals(bufferSize))
                return;

            bufferSize = (w, h);

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
            var (w, h) = bufferSize;
            diffuseBuffer.Resize(w, h, PixelInternalFormat.Rgba);
            normalBuffer.Resize(w, h, PixelInternalFormat.Rgba16f);
            depthBuffer.Resize(w, h, PixelInternalFormat.R16f);
            accumBuffer.Resize(w, h, PixelInternalFormat.Rgb16f);
            resizeDepthMask(w, h);
        }

        private void resizeDepthMask(int w, int h)
        {
            GL.BindTexture(TextureTarget.Texture2D, depthMaskBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, w, h, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private static Texture createTexture()
        {
            var texture = new Texture(1, 1);
            texture.SetParameters(TextureMinFilter.Linear, TextureMagFilter.Linear,
                TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            return texture;
        }

        private static Texture createDepthTexture()
        {
            var texture = createTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.DepthTextureMode, (int)All.Intensity);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRToTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Lequal);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }

    }
}
