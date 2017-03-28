using amulware.Graphics;
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

        private bool gameStarted;

        public LobbyScreen(ScreenLayerCollection parent, GeometryManager geometries, Logger logger)
            : base(parent, geometries, .5f, .5f, true)
        {
            this.logger = logger;
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            if (gameStarted)
                return true;

            if (InputManager.IsKeyHit(Key.Enter))
                startGame();

            return false;
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
            Parent.AddScreenLayerOnTopOf(this, new GameUI(Parent, Geometries, logger));
            gameStarted = true;
            Destroy();
        }
    }
}