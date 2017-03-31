using System;
using amulware.Graphics;
using Bearded.TD.Game.UI;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.Utilities.Console;
using Bearded.Utilities;
using Bearded.Utilities.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace Bearded.TD
{
    class TheGame : Program
    {
        private readonly Logger logger;

        private RenderContext renderContext;
        private ScreenManager screenManager;

        public TheGame(Logger logger)
         : base(1280, 720, GraphicsMode.Default, "Bearded.TD",
             GameWindowFlags.Default, DisplayDevice.Default,
             3, 2, GraphicsContextFlags.Default)
        {
            this.logger = logger;
        }

        protected override void OnLoad(EventArgs e)
        {
            ConsoleCommands.Initialise();

            renderContext = new RenderContext();

            InputManager.Initialize(Mouse);

            screenManager = new ScreenManager();
            
            screenManager.AddScreenLayerOnTop(new LobbyScreen(screenManager, renderContext.Geometries, logger));
            //screenManager.AddScreenLayerOnTop(new GameUI(screenManager, renderContext.Geometries, logger));
            screenManager.AddScreenLayerOnTop(new ConsoleScreenLayer(screenManager, renderContext.Geometries, logger));

            KeyPress += (sender, args) => screenManager.RegisterPressedCharacter(args.KeyChar);

            OnResize(EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            screenManager.OnResize(new ViewportSize(Width,Height));
        }

        protected override void OnUpdate(UpdateEventArgs e)
        {
            if (Focused)
                lock (UIEventProcessLock)
                {
                    InputManager.Update();
                }

            if ((InputManager.IsKeyPressed(Key.AltLeft) && InputManager.IsKeyHit(Key.F4))
                || InputManager.IsKeyPressed(Key.Escape))
            {
                Close();
            }

            screenManager.Update(e);
        }

        protected override void OnRender(UpdateEventArgs e)
        {
            renderContext.Compositor.PrepareForFrame();
            screenManager.Render(renderContext);
            renderContext.Compositor.FinalizeFrame();

            SwapBuffers();
        }
    }
}
