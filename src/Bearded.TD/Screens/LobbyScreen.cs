using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.UI;
using Bearded.TD.Networking.Lobby;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.Utilities.Input;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Screens
{
    class LobbyScreen : UIScreenLayer
    {
        private readonly LobbyManager lobbyManager;
        private readonly InputManager inputManager;
        
        public LobbyScreen(ScreenLayerCollection parent, GeometryManager geometries, LobbyManager lobbyManager, InputManager inputManager)
            : base(parent, geometries, .5f, .5f, true)
        {
            this.lobbyManager = lobbyManager;
            this.inputManager = inputManager;
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            if (inputState.InputManager.IsKeyHit(Key.Enter))
                lobbyManager.ToggleReadyState();

            return false;
        }

        public override void Update(UpdateEventArgs args)
        {
            lobbyManager.Update(args);
            if (lobbyManager.GameStarted)
                startGame();
        }

        public override void Draw()
        {
            var txtGeo = Geometries.ConsoleFont;

            txtGeo.Color = Color.White;
            txtGeo.SizeCoefficient = Vector2.One;
            txtGeo.Height = 48;
            txtGeo.DrawString(Vector2.Zero, "Press [enter] to start", .5f, .5f);
            txtGeo.DrawString(64 * Vector2.UnitY, $"Player count: {lobbyManager.Players.Count}", .5f, .5f);
            txtGeo.DrawString(128 * Vector2.UnitY, string.Join(", ", lobbyManager.Players.Select(p => p.Player.Name)), .5f, .5f);
        }

        private void startGame()
        {
            Parent.AddScreenLayerOnTopOf(this, new GameUI(Parent, Geometries, lobbyManager.BuildInstance(inputManager), inputManager));
            Destroy();
        }
    }
}