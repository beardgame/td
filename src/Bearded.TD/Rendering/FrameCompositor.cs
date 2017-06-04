using amulware.Graphics;
using Bearded.TD.Screens;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering
{
    class FrameCompositor
    {
        private readonly SurfaceManager surfaces;
        private int renderWidth;
        private int renderHeight;

        public FrameCompositor(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;
        }

        public void PrepareForFrame()
        {
            GL.Viewport(0, 0, renderWidth, renderHeight);

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

            surfaces.ConsoleBackground.Render();
            surfaces.ConsoleFontSurface.Render();
        }

        public void FinalizeFrame()
        {
        }

        public void SetViewportSize(ViewportSize viewportSize)
        {
            renderWidth = viewportSize.Width;
            renderHeight = viewportSize.Height;
        }
    }
}
