﻿using System;
using amulware.Graphics;
using Bearded.TD.Meta;
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
            setMatricesFrom(layer);
        }

        private void setMatricesFrom(IRenderLayer layer)
        {
            surfaces.ViewMatrix.Matrix = layer.ViewMatrix;
            surfaces.ProjectionMatrix.Matrix = layer.ProjectionMatrix;
        }

        private void renderWithOptions(RenderOptions options)
        {
            if (options.RenderDeferred)
            {
                renderDeferred();
            }

            renderConsoleSurfaces();
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
