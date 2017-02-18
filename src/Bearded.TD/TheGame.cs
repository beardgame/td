using System;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Rendering;
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
        private RenderContext renderContext;
        private readonly Logger logger;

        private GameState gameState;
        private GameRunner gameRunner;
        private ScreenLayer consoleLayer;
        private GameScreenLayer gameScreenLayer;

        public TheGame(Logger logger)
         : base(1280, 720, GraphicsMode.Default, "Bearded.TD",
             GameWindowFlags.Default, DisplayDevice.Default,
             3, 2, GraphicsContextFlags.Default)
        {
            this.logger = logger;
        }

        protected override void OnLoad(EventArgs e)
        {
            Commands.Initialise();

            renderContext = new RenderContext();

            InputManager.Initialize(Mouse);


            var meta = new GameMeta(logger);

            gameState = new GameState(meta);
            gameRunner = new GameRunner(gameState);
            consoleLayer = new ConsoleScreenLayer(logger, renderContext.Geometries);
            gameScreenLayer = new GameScreenLayer(gameState, renderContext.Geometries);
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
            renderContext.Compositor.PrepareForFrame();
            renderContext.Compositor.RenderLayer(consoleLayer);
            renderContext.Compositor.FinalizeFrame();

            SwapBuffers();
        }
    }
}
