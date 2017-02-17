using System;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.Console;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD
{
    class TheGame : Program
    {
        private RenderContext renderContext;

        public TheGame()
         : base(1280, 720, GraphicsMode.Default, "Bearded.TD",
             GameWindowFlags.Default, DisplayDevice.Default,
             3, 2, GraphicsContextFlags.Default)
        {

        }

        protected override void OnLoad(EventArgs e)
        {
            Commands.Initialise();

            renderContext = new RenderContext();
        }

        protected override void OnResize(EventArgs e)
        {

        }

        protected override void OnUpdate(UpdateEventArgs e)
        {

        }

        protected override void OnRender(UpdateEventArgs e)
        {
            renderContext.Compositor.PrepareForFrame();

            renderContext.Compositor.FinalizeFrame();

            this.SwapBuffers();
        }

    }
}
