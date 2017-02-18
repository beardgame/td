using amulware.Graphics;
using Bearded.TD.Game;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering
{
    class FrameCompositor
    {
        private readonly SurfaceManager surfaces;

        public FrameCompositor(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;
        }

        public void PrepareForFrame()
        {
            var argb = Color.SkyBlue;
            GL.ClearColor(argb.R / 255f, argb.G / 255f, argb.B / 255f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            GL.CullFace(CullFaceMode.FrontAndBack);

            GL.Enable(EnableCap.Blend);
            SurfaceBlendSetting.PremultipliedAlpha.Set(null);
        }

        public void RenderGame(GameScreenLayer renderer)
        {
            renderer.Draw();
        }

        public void RenderLayer(ScreenLayer layer)
        {
            layer.Draw();

            surfaces.ViewMatrix.Matrix = layer.GetViewMatrix();
            surfaces.ProjectionMatrix.Matrix = layer.GetProjectionMatrix();

            surfaces.ConsoleBackground.Render();
            surfaces.ConsoleFontSurface.Render();
        }

        public void FinalizeFrame()
        {
        }
    }
}
