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
        }

        public void RenderGame(GameRenderer renderer)
        {
            renderer.Draw();
        }

        public void FinalizeFrame()
        {
        }
    }
}