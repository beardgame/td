using System;
using amulware.Graphics;
using Bearded.TD.Utilities.Console;
using Bearded.Utilities.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Bearded.TD
{
    class TheGame : Program
    {
        public TheGame()
         : base(1280, 720, GraphicsMode.Default, "Bearded.TD",
             GameWindowFlags.Default, DisplayDevice.Default,
             3, 2, GraphicsContextFlags.Default)
        {

        }

        protected override void OnLoad(EventArgs e)
        {
            Commands.Initialise();
            InputManager.Initialize(Mouse);
        }

        protected override void OnResize(EventArgs e)
        {

        }

        protected override void OnUpdate(UpdateEventArgs e)
        {
            InputManager.Update();

            if (InputManager.IsKeyPressed(Key.AltLeft) && InputManager.IsKeyHit(Key.F4))
            {
                Close();
            }
        }

        protected override void OnRender(UpdateEventArgs e)
        {
            var argb = Color.SkyBlue;
            GL.ClearColor(argb.R / 255f, argb.G / 255f, argb.B / 255f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            SwapBuffers();
        }
    }
}
