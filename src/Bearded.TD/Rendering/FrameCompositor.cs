using System;
using amulware.Graphics;
using amulware.Graphics.ShaderManagement;
using Bearded.TD.Meta;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.Utilities.IO;
using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering
{
    class FrameCompositor
    {
        private readonly Logger logger;
        private readonly SurfaceManager surfaces;
        private readonly DeferredRenderer deferredRenderer;

        private DateTime nextShaderReloadTime = DateTime.UtcNow;
        public ViewportSize ViewPort { get; private set; }

        public FrameCompositor(Logger logger, SurfaceManager surfaces)
        {
            this.logger = logger;
            this.surfaces = surfaces;
            deferredRenderer = new DeferredRenderer(surfaces);
        }

        public void SetViewportSize(ViewportSize viewPort)
        {
            ViewPort = viewPort;
            deferredRenderer.OnResize(viewPort);
        }

        public void PrepareForFrame()
        {
#if DEBUG
            reloadShadersIfNeeded();
#endif

            GL.Viewport(0, 0, ViewPort.Width, ViewPort.Height);
            GL.Disable(EnableCap.ScissorTest);

            var argb = Color.Black;
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

            var report = surfaces.Shaders.TryReloadAll();

            logShaderReloadReport(report);
        }

        private void logShaderReloadReport(ShaderReloadReport report)
        {
            if (!report.TriedReloadingAnything)
                return;

            if (report.ReloadedShaderCount > 0)
                logger.Debug?.Log($"Reloaded {report.ReloadedShaderCount} shaders.");

            if (report.ReloadedProgramCount > 0)
                logger.Debug?.Log($"Reloaded {report.ReloadedProgramCount} shader programs.");

            logShaderReloadErrors(report);
        }

        private void logShaderReloadErrors(ShaderReloadReport report)
        {
            if (report.ReloadExceptions.Count <= 0)
                return;

            logger.Error?.Log($"Failed to reload shaders:");

            foreach (var exception in report.ReloadExceptions)
            {
                foreach (var line in exception.Message.Split('\n'))
                {
                    logger.Error?.Log(line);
                }
            }
        }

        public void RenderLayer(IRenderLayer layer)
        {
            prepareForRendering(layer);
            setViewportFrom(layer.RenderOptions);
            renderSurfaces(layer);
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
            surfaces.Time.Float = deferredRenderLayer.Time;

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

        private void setViewportFrom(RenderOptions options)
        {
            if (options.OverrideViewport.HasValue)
            {
                var ((x, y), (w, h)) = options.OverrideViewport.Value;
                GL.Enable(EnableCap.ScissorTest);
                GL.Scissor(x, y, w, h);
            }
            else
            {
                GL.Disable(EnableCap.ScissorTest);
            }
        }

        private void renderSurfaces(IRenderLayer layer)
        {
            if (layer is IDeferredRenderLayer deferredLayer)
            {
                renderDeferred(deferredLayer);
            }

            renderConsoleSurfaces();
        }

        private void renderDeferred(IDeferredRenderLayer deferredLayer)
        {
            deferredRenderer.Render(deferredLayer);

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
