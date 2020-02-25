using System;
using System.Collections.ObjectModel;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Meta;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using OpenTK.Graphics.OpenGL;
using static Bearded.TD.Content.Models.SpriteDrawGroup;

namespace Bearded.TD.Rendering
{
    class DeferredRenderer
    {
        private static readonly SpriteDrawGroup[] worldDrawGroups = { Building, Unit };
        private static readonly SpriteDrawGroup[] postLightGroups = { Particle, Unknown };

        public static readonly ReadOnlyCollection<SpriteDrawGroup> AllDrawGroups =
            new [] {worldDrawGroups, postLightGroups}
                .SelectMany(group => group)
                .ToList().AsReadOnly();

        private readonly SurfaceManager surfaces;
        private ViewportSize viewport;
        private (int width, int height) bufferSize;
        private float cameraDistance;
        private bool needsResize;

        private readonly Texture diffuseBuffer = createTexture(); // rgba
        private readonly Texture normalBuffer = createTexture(); // xyz
        private readonly Texture depthBuffer = createTexture(); // z (0-1, camera space)
        private readonly Texture lightAccumBuffer = createTexture(); // rgb
        private readonly Texture depthMaskBuffer = createDepthTexture();
        private readonly Texture compositionBuffer = createTexture(); // rgba

        private readonly RenderTarget gTarget = new RenderTarget();
        private readonly RenderTarget lightAccumTarget = new RenderTarget();
        private readonly RenderTarget compositionTarget = new RenderTarget();

        private readonly IndexedSurface<UVColorVertexData>[] debugSurfaces;

        private readonly PostProcessSurface compositeSurface;
        private readonly PostProcessSurface copyToTargetSurface;

        public DeferredRenderer(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;

            gTarget.Attach(FramebufferAttachment.ColorAttachment0, diffuseBuffer);
            gTarget.Attach(FramebufferAttachment.ColorAttachment1, normalBuffer);
            gTarget.Attach(FramebufferAttachment.ColorAttachment2, depthBuffer);
            gTarget.Attach(FramebufferAttachment.DepthAttachment, depthMaskBuffer);
            bind(gTarget, (0, 0));
            GL.DrawBuffers(3, new []
            {
                DrawBuffersEnum.ColorAttachment0,
                DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2,
            });
            bind(null, (0, 0));

            lightAccumTarget.Attach(FramebufferAttachment.ColorAttachment0, lightAccumBuffer);

            compositionTarget.Attach(FramebufferAttachment.ColorAttachment0, compositionBuffer);
            compositionTarget.Attach(FramebufferAttachment.DepthAttachment, depthMaskBuffer);

            compositeSurface = new PostProcessSurface()
                .WithShader(surfaces.Shaders["deferred/compose"])
                .AndSettings(
                    new TextureUniform("albedoTexture", diffuseBuffer, TextureUnit.Texture0),
                    new TextureUniform("lightTexture", lightAccumBuffer, TextureUnit.Texture1)
                );

            copyToTargetSurface = new PostProcessSurface()
                .WithShader(surfaces.Shaders["deferred/copy"])
                .AndSettings(
                    new TextureUniform("inputTexture", compositionBuffer, TextureUnit.Texture0)
                );

            debugSurfaces = new[] {diffuseBuffer, normalBuffer, depthBuffer, lightAccumBuffer}
                .Select(createDebugSurface).ToArray();

            surfaces.InjectDeferredBuffer(normalBuffer, depthBuffer);
        }

        private IndexedSurface<UVColorVertexData> createDebugSurface(Texture buffer)
            => new IndexedSurface<UVColorVertexData>()
                .WithShader(surfaces.Shaders["deferred/debug"])
                .AndSettings(new TextureUniform("bufferTexture", buffer));

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

                u += width;

                surface.Render();
            }
        }

        public void Render(IDeferredRenderLayer deferredLayer, RenderTarget target = null)
        {
            resizeForCameraDistance(deferredLayer.CameraDistance);

            resizeIfNeeded();

            renderWorldToGBuffers(deferredLayer.DeferredSurfaces);

            renderLightsToAccumBuffer();

            compositeLightsAndGBuffers();

            renderFluidsToComposition(deferredLayer.DeferredSurfaces);

            renderPostLightDrawGroupsToComposition(deferredLayer.DeferredSurfaces);

            copyCompositionTo(target);

            resetChangedSettings();
        }

        private void renderWorldToGBuffers(ContentSurfaceManager contentSurfaces)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            GL.Disable(EnableCap.Blend);

            bind(gTarget, bufferSize);
            clearColorAndDepth();

            contentSurfaces.LevelGeometry.RenderAll();

            GL.DepthMask(false);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);

            renderDrawGroups(contentSurfaces, worldDrawGroups);
        }

        private void renderLightsToAccumBuffer()
        {
            bind(lightAccumTarget, bufferSize);

            clearColor();

            surfaces.PointLights.Render();
            surfaces.Spotlights.Render();
        }

        private void compositeLightsAndGBuffers()
        {
            bind(compositionTarget, bufferSize);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);

            compositeSurface.Render();
        }

        private void renderFluidsToComposition(ContentSurfaceManager contentSurfaces)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            foreach (var fluid in contentSurfaces.FluidGeometries)
            {
                fluid.Render();
            }
        }

        private void renderPostLightDrawGroupsToComposition(ContentSurfaceManager contentSurfaces)
        {
            renderDrawGroups(contentSurfaces, postLightGroups);
        }

        private void copyCompositionTo(RenderTarget target)
        {
            bind(target, (viewport.Width, viewport.Height));

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);

            copyToTargetSurface.Render();
        }

        private void resetChangedSettings()
        {
            GL.Enable(EnableCap.Blend);
        }

        private static void bind(RenderTarget target, (int width, int height) viewport)
        {
            GL.Viewport(0, 0, viewport.width, viewport.height);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, target);
        }

        private static void clearColorAndDepth()
        {
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private static void clearColor(float r = 0, float g = 0, float b = 0, float a = 0)
        {
            GL.ClearColor(r, g, b, a);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        private static void renderDrawGroups(ContentSurfaceManager contentSurfaces, SpriteDrawGroup[] drawGroups)
        {
            foreach (var drawGroup in drawGroups)
            {
                foreach (var surface in contentSurfaces.SurfacesFor(drawGroup))
                {
                    surface.Render();
                }
            }
        }

        public void OnResize(ViewportSize newViewport)
        {
            var bufferSizeFactor = UserSettings.Instance.Graphics.SuperSample;

            var w = (int) (newViewport.Width * bufferSizeFactor);
            var h = (int) (newViewport.Height * bufferSizeFactor);

            if (newViewport == viewport && (w, h).Equals(bufferSize))
                return;

            bufferSize = (w, h);

            viewport = newViewport;
            needsResize = true;
        }

        private void resizeForCameraDistance(float cameraDistance)
        {
            var screenPixelsPerTile = viewport.Height * 0.5f / cameraDistance;

            var scale = Math.Min(1, Constants.Rendering.PixelsPerTile / screenPixelsPerTile);

            var bufferSizeFactor = UserSettings.Instance.Graphics.SuperSample;

            var w = (int) (viewport.Width * bufferSizeFactor * scale);
            var h = (int) (viewport.Height * bufferSizeFactor * scale);

            if ((w, h).Equals(bufferSize))
                return;

            bufferSize = (w, h);
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
            normalBuffer.Resize(w, h, PixelInternalFormat.Rgba);
            depthBuffer.Resize(w, h, PixelInternalFormat.R16f);
            lightAccumBuffer.Resize(w, h, PixelInternalFormat.Rgb16f);
            compositionBuffer.Resize(w, h, PixelInternalFormat.Rgba);
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
            texture.SetParameters(TextureMinFilter.Linear, TextureMagFilter.Nearest,
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
