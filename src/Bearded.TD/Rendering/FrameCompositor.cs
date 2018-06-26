using System;
using amulware.Graphics;
using Bearded.TD.Meta;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering
{
    class FrameCompositor
    {
        private DateTime nextShaderReloadTime = DateTime.UtcNow;
        private readonly SurfaceManager surfaces;
        private readonly DeferredRenderer deferredRenderer;
        private ViewportSize viewPort;

        public FrameCompositor(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;
            deferredRenderer = new DeferredRenderer(surfaces);
        }

        public void SetViewportSize(ViewportSize viewPort)
        {
            this.viewPort = viewPort;
            deferredRenderer.OnResize(viewPort);
        }

        public void PrepareForFrame()
        {
            reloadShadersIfNeeded();

            GL.Viewport(0, 0, viewPort.Width, viewPort.Height);

            var argb = Color.DarkSlateGray * 0.5f;
            GL.ClearColor(argb.R / 255f, argb.G / 255f, argb.B / 255f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            GL.CullFace(CullFaceMode.FrontAndBack);

            GL.Enable(EnableCap.Blend);
            SurfaceBlendSetting.PremultipliedAlpha.Set(null);
        }

        private void reloadShadersIfNeeded()
        {
            var now = DateTime.UtcNow;

            if (nextShaderReloadTime > now)
                return;

            nextShaderReloadTime = now + TimeSpan.FromSeconds(1);
            
            // TODO: debug why this doesn't work and print something to the console every time shaders are reloaded or fail to
            surfaces.Shaders.TryReloadAll();
        }

        public void RenderLayer(IRenderLayer layer)
        {
            prepareForRendering(layer);
            renderWithOptions(layer.RenderOptions);
        }

        private void prepareForRendering(IRenderLayer layer)
        {
            layer.Draw();
            setUniformsFrom(layer);
        }

        private void setUniformsFrom(IRenderLayer layer)
        {
            surfaces.ViewMatrix.Matrix = layer.ViewMatrix;
            surfaces.ProjectionMatrix.Matrix = layer.ProjectionMatrix;

            if (layer is IDeferredRenderLayer deferredLayer)
                setUniformsFrom(deferredLayer);
        }

        private void setUniformsFrom(IDeferredRenderLayer deferredRenderLayer)
        {
            surfaces.FarPlaneDistance.Float = deferredRenderLayer.FarPlaneDistance;

            setFarPlaneParameters(deferredRenderLayer);
        }

        private void setFarPlaneParameters(IDeferredRenderLayer deferredRenderLayer)
        {
            var projection = deferredRenderLayer.ProjectionMatrix;
            var view = deferredRenderLayer.ViewMatrix;

            var cameraTranslation = view.ExtractTranslation();
            var viewWithoutTranslation = view.ClearTranslation();

            var projectionView = viewWithoutTranslation * projection;
            var projectionViewInverted = projectionView.Inverted();

            // check if multiplication by the inverted view matrix is working correctly
            var baseCorner = new Vector4(-1, -1, 1, 1) * projectionViewInverted;
            baseCorner = baseCorner / baseCorner.W;

            var xCorner = new Vector4(1, -1, 1, 1) * projectionViewInverted;
            xCorner = xCorner / xCorner.W;

            var yCorner = new Vector4(-1, 1, 1, 1) * projectionViewInverted;
            yCorner = yCorner / yCorner.W;

            surfaces.FarPlaneBaseCorner.Vector = baseCorner.Xyz;
            surfaces.FarPlaneUnitX.Vector = xCorner.Xyz - baseCorner.Xyz;
            surfaces.FarPlaneUnitY.Vector = yCorner.Xyz - baseCorner.Xyz;
            surfaces.CameraPosition.Vector = cameraTranslation;
        }

        private void renderWithOptions(RenderOptions options)
        {
            setViewportFrom(options);

            if (options.RenderDeferred)
            {
                renderDeferred();
            }

            renderConsoleSurfaces();
        }

        private void setViewportFrom(RenderOptions options)
        {
            var ((x, y), (w, h)) = options.OverrideViewport ?? ((0, 0), (viewPort.Width, viewPort.Height));

            GL.Viewport(x, y, w, h);
        }

        private void renderDeferred()
        {
            deferredRenderer.Render();

            if (UserSettings.Instance.Debug.Deferred)
                deferredRenderer.RenderDebug();
        }

        private void renderConsoleSurfaces()
        {
            surfaces.Primitives.Render();
            surfaces.UIFontSurface.Render();
            surfaces.ConsoleBackground.Render();
            surfaces.ConsoleFontSurface.Render();
        }

        public void FinalizeFrame()
        {
        }
    }
}
