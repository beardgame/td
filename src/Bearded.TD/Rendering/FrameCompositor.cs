using amulware.Graphics;
using Bearded.TD.Screens;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering
{
    class FrameCompositor
    {
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

        public void RenderLayer(ScreenLayer layer)
        {
            layer.Draw();

            surfaces.ViewMatrix.Matrix = layer.ViewMatrix;
            surfaces.ProjectionMatrix.Matrix = layer.ProjectionMatrix;

            // TODO: need to render deferred part here(?) if needed

            surfaces.ConsoleBackground.Render();
            surfaces.ConsoleFontSurface.Render();
        }

        private void renderDeferred()
        {
            // TODO: how do we get here?

            deferredRenderer.Render();
        }

        public void FinalizeFrame()
        {
        }
    }
}
