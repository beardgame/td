using System;
using amulware.Graphics;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using Bearded.TD.Utilities.Console;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace Bearded.TD
{
    class TheGame : Program
    {
        private readonly Logger logger;

        private InputManager inputManager;
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

            inputManager = new InputManager(Mouse);

            screenManager = new ScreenManager(inputManager);
            
            screenManager.AddScreenLayerOnTop(new StartScreen(screenManager, renderContext.Geometries, logger, inputManager));
            screenManager.AddScreenLayerOnTop(new ConsoleScreenLayer(screenManager, renderContext.Geometries, logger));

            KeyPress += (sender, args) => screenManager.RegisterPressedCharacter(args.KeyChar);

            OnResize(EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            var viewPort = new ViewportSize(Width, Height);
            screenManager.OnResize(viewPort);
            renderContext.OnResize(viewPort);
        }

        protected override void OnUpdateUIThread()
        {
            inputManager.ProcessEventsAsync();
        }

        protected override void OnUpdate(UpdateEventArgs e)
        {
            inputManager.Update(Focused);

            if ((inputManager.IsKeyPressed(Key.AltLeft) && inputManager.IsKeyHit(Key.F4))
                || inputManager.IsKeyPressed(Key.Escape))
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
