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
        private (int width, int height) bufferSizeLowRes;
        private (int width, int height) bufferSize;
        private bool needsResize;

        private readonly Texture diffuseBufferLowRes = createTexture(); // rgba
        private readonly Texture normalBufferLowRes = createTexture(); // xyz
        private readonly Texture depthBufferLowRes = createTexture(); // z (0-1, camera space)

        private readonly Texture diffuseBuffer = createTexture(); // rgba
        private readonly Texture normalBuffer = createTexture(); // xyz
        private readonly Texture depthBuffer = createTexture(); // z (0-1, camera space)


        private readonly Texture lightAccumBuffer = createTexture(); // rgb
        private readonly Texture depthMaskBuffer = createDepthTexture();
        private readonly Texture compositionBuffer = createTexture(); // rgba

        private readonly RenderTarget gTargetLowRes = new RenderTarget();
        private readonly RenderTarget gTarget = new RenderTarget();
        private readonly RenderTarget lightAccumTarget = new RenderTarget();
        private readonly RenderTarget compositionTarget = new RenderTarget();

        private readonly IndexedSurface<UVColorVertexData>[] debugSurfaces;

        private readonly PostProcessSurface compositeSurface;
        private readonly PostProcessSurface copyToTargetSurface;

        private readonly RenderTarget copyDiffuseTarget = new RenderTarget();
        private readonly RenderTarget copyNormalTarget = new RenderTarget();
        private readonly RenderTarget copyDepthTarget = new RenderTarget();
        private readonly PostProcessSurface copyDiffuseSurface;
        private readonly PostProcessSurface copyNormalSurface;
        private readonly PostProcessSurface copyDepthSurface;

        public DeferredRenderer(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;

            gTargetLowRes.Attach(FramebufferAttachment.ColorAttachment0, diffuseBufferLowRes);
            gTargetLowRes.Attach(FramebufferAttachment.ColorAttachment1, normalBufferLowRes);
            gTargetLowRes.Attach(FramebufferAttachment.ColorAttachment2, depthBufferLowRes);
            gTargetLowRes.Attach(FramebufferAttachment.DepthAttachment, depthMaskBuffer);
            bind(gTargetLowRes, (0, 0));
            GL.DrawBuffers(3, new []
            {
                DrawBuffersEnum.ColorAttachment0,
                DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2,
            });
            bind(null, (0, 0));

            gTarget.Attach(FramebufferAttachment.ColorAttachment0, diffuseBuffer);
            gTarget.Attach(FramebufferAttachment.ColorAttachment1, normalBuffer);
            gTarget.Attach(FramebufferAttachment.ColorAttachment2, depthBuffer);
            bind(gTarget, (0, 0));
            GL.DrawBuffers(3, new []
            {
                DrawBuffersEnum.ColorAttachment0,
                DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2,
            });
            bind(null, (0, 0));

            copyDiffuseTarget.Attach(FramebufferAttachment.ColorAttachment0, diffuseBuffer);
            bind(copyDiffuseTarget, (0, 0));
            GL.DrawBuffers(3, new [] {DrawBuffersEnum.ColorAttachment0});
            bind(null, (0, 0));

            copyNormalTarget.Attach(FramebufferAttachment.ColorAttachment0, normalBuffer);
            bind(copyNormalTarget, (0, 0));
            GL.DrawBuffers(3, new [] {DrawBuffersEnum.ColorAttachment0});
            bind(null, (0, 0));

            copyDepthTarget.Attach(FramebufferAttachment.ColorAttachment0, depthBuffer);
            bind(copyDepthTarget, (0, 0));
            GL.DrawBuffers(3, new [] {DrawBuffersEnum.ColorAttachment0});
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


            copyDiffuseSurface = new PostProcessSurface()
                .WithShader(surfaces.Shaders["deferred/copy"])
                .AndSettings(
                    new TextureUniform("inputTexture", diffuseBufferLowRes, TextureUnit.Texture0)
                );
            copyNormalSurface = new PostProcessSurface()
                .WithShader(surfaces.Shaders["deferred/copy"])
                .AndSettings(
                    new TextureUniform("inputTexture", normalBufferLowRes, TextureUnit.Texture0)
                );
            copyDepthSurface = new PostProcessSurface()
                .WithShader(surfaces.Shaders["deferred/copy"])
                .AndSettings(
                    new TextureUniform("inputTexture", depthBufferLowRes, TextureUnit.Texture0)
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

            bind(gTargetLowRes, bufferSizeLowRes);
            clearColorAndDepth();

            contentSurfaces.LevelGeometry.RenderAll();

            GL.DepthMask(false);
            GL.Disable(EnableCap.CullFace);


            bind(copyDiffuseTarget, bufferSize);
            copyDiffuseSurface.Render();

            bind(copyNormalTarget, bufferSize);
            copyNormalSurface.Render();

            bind(copyDepthTarget, bufferSize);
            copyDepthSurface.Render();




            bind(gTarget, bufferSize);

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
            viewport = newViewport;
        }

        private void resizeForCameraDistance(float cameraDistance)
        {
            updateBufferSize(ref bufferSizeLowRes, cameraDistance, Constants.Rendering.PixelsPerTileLevelResolution);
            updateBufferSize(ref bufferSize, cameraDistance, Constants.Rendering.PixelsPerTileSpriteResolution);
        }

        private void updateBufferSize(ref (int, int) size, float cameraDistance, float pixelsPerTile)
        {
            var newSize = calculateBufferSize(cameraDistance, pixelsPerTile);

            if (newSize.Equals(size))
                return;

            size = newSize;
            needsResize = true;
        }

        private (int w, int h) calculateBufferSize(float cameraDistance, float pixelsPerTile)
        {
            var screenPixelsPerTile = viewport.Height * 0.5f / cameraDistance;

            var scale = Math.Min(1, pixelsPerTile / screenPixelsPerTile);

            var bufferSizeFactor = UserSettings.Instance.Graphics.SuperSample;

            var w = (int) (viewport.Width * bufferSizeFactor * scale);
            var h = (int) (viewport.Height * bufferSizeFactor * scale);

            return (w, h);
        }

        private void resizeIfNeeded()
        {
            if (needsResize)
                resize();

            needsResize = false;
        }

        private void resize()
        {
            var (w, h) = bufferSizeLowRes;
            diffuseBufferLowRes.Resize(w, h, PixelInternalFormat.Rgba);
            normalBufferLowRes.Resize(w, h, PixelInternalFormat.Rgba);
            depthBufferLowRes.Resize(w, h, PixelInternalFormat.R16f);

            (w, h) = bufferSize;
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
