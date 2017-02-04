using System;
using amulware.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace Bearded.TD
{
    public class TheGame : Program
    {
        public TheGame()
         : base(1280, 720, GraphicsMode.Default, "Bearded.TD",
             GameWindowFlags.Default, DisplayDevice.Default,
             3, 2, GraphicsContextFlags.Default)
        {

        }

        protected override void OnLoad(EventArgs e)
        {

        }

        protected override void OnUpdate(UpdateEventArgs e)
        {
        }

        protected override void OnRender(UpdateEventArgs e)
        {
        }

    }
}