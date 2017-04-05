using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.UI;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.Utilities;
using Bearded.Utilities.Input;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Screens
{
    class LobbyScreen : UIScreenLayer
    {
        private readonly Logger logger;
        private readonly Player me;

        private bool gameStarted;

        public LobbyScreen(ScreenLayerCollection parent, GeometryManager geometries, Logger logger)
            : base(parent, geometries, .5f, .5f, true)
        {
            this.logger = logger;
            me = new Player(Color.Gray);
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            if (gameStarted)
                return true;

            if (InputManager.IsKeyHit(Key.Enter))
                startGame();

            return false;
        }

        public override void Update(UpdateEventArgs args)
        {
            if (gameStarted)
                return;

            // Update network stuff here.
        }

        public override void Draw()
        {
            var txtGeo = Geometries.ConsoleFont;

            txtGeo.Color = Color.White;
            txtGeo.SizeCoefficient = Vector2.One;
            txtGeo.Height = 48;
            txtGeo.DrawString(Vector2.Zero, "Press [enter] to start", .5f, .5f);
        }

        private void startGame()
        {
            // these are different for clients
            var commandDispatcher = new ServerCommandDispatcher(new DefaultCommandExecutor());
            var requestDispatcher = new ServerRequestDispatcher(commandDispatcher);
            var dispatcher = new ServerDispatcher(commandDispatcher);

            var meta = new GameMeta(logger, dispatcher);

            var gameState = GameStateBuilder.Generate(meta, new DefaultTilemapGenerator(logger));
            var gameInstance = new GameInstance(
                me,
                gameState,
                new GameCamera(meta, gameState.Level.Tilemap.Radius),
                requestDispatcher
                );

            Parent.AddScreenLayerOnTopOf(this, new GameUI(Parent, Geometries, gameInstance));
            gameStarted = true;
            Destroy();
        }
    }
}