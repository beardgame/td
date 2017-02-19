using System;
using amulware.Graphics;
using Bearded.TD.Game;
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
            Commands.Initialise();

            renderContext = new RenderContext();

            InputManager.Initialize(Mouse);

            var meta = new GameMeta(logger);

            var gameState = GameStateBuilder.Empty(meta);
            var gameInstance = new GameInstance(
                gameState,
                new GameCamera(meta, gameState.Level.Tilemap.Radius));
            var gameRunner = new GameRunner(gameInstance);

            screenManager = new ScreenManager();
            screenManager.AddScreenLayer(new GameScreenLayer(gameInstance, gameRunner, renderContext.Geometries));
            screenManager.AddScreenLayer(new ConsoleScreenLayer(logger, renderContext.Geometries));

            OnResize(EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            screenManager.OnResize(new ViewportSize(Width, Height));
        }

        protected override void OnUpdate(UpdateEventArgs e)
        {
            if (Focused)
                lock (UIEventProcessLock)
                {
                    InputManager.Update();
                }

            if (InputManager.IsKeyPressed(Key.AltLeft) && InputManager.IsKeyHit(Key.F4))
            {
                Close();
            }

            screenManager.Update(e);
        }

        protected override void OnRender(UpdateEventArgs e)
        {
            renderContext.Compositor.PrepareForFrame();
            screenManager.Draw(renderContext);
            renderContext.Compositor.FinalizeFrame();

            SwapBuffers();
        }
    }
}
