using amulware.Graphics;
using Bearded.TD.Networking.Lobby;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Screens
{
    class StartScreen : UIScreenLayer
    {
        private readonly Logger logger;
        private readonly InputManager inputManager;

        public StartScreen(ScreenLayerCollection parent, GeometryManager geometries, Logger logger, InputManager inputManager) : base(parent, geometries, .5f, .5f, true)
        {
            this.logger = logger;
            this.inputManager = inputManager;
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            if (inputState.InputManager.IsKeyHit(Key.Number1))
                startGame();
            else if (inputState.InputManager.IsKeyHit(Key.Number2))
                startServerLobby();
            else if (inputState.InputManager.IsKeyHit(Key.Number3))
                startConnect();

            return false;
        }

        public override void Draw()
        {
            var txtGeo = Geometries.ConsoleFont;

            txtGeo.Color = Color.White;
            txtGeo.SizeCoefficient = Vector2.One;
            txtGeo.Height = 48;
            txtGeo.DrawString(-64 * Vector2.UnitY, "Press 1 to start game in single player", .5f, .5f);
            txtGeo.DrawString(Vector2.Zero, "Press 2 to host lobby", .5f, .5f);
            txtGeo.DrawString(64 * Vector2.UnitY, "Press 3 to join lobby", .5f, .5f);
        }

        private void startGame()
        {
            logger.Error.Log("Sorry not yet");
            //Destroy();
        }

        private void startServerLobby()
        {
            Parent.AddScreenLayerOnTopOf(this, new LobbyScreen(Parent, Geometries, new ServerLobbyManager(logger), inputManager));
            Destroy();
        }

        private void startConnect()
        {
            Parent.AddScreenLayerOnTopOf(this, new ConnectToLobbyScreen(Parent, Geometries, logger, inputManager));
            Destroy();
        }
    }
}