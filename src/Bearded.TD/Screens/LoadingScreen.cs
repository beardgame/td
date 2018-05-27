using System;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Model.Loading;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Input;
using OpenTK;

namespace Bearded.TD.Screens
{
    class LoadingScreen : UIScreenLayer
    {
        private readonly LoadingManager loadingManager;
        private readonly InputManager inputManager;

        public LoadingScreen(
            ScreenLayerCollection parent, GeometryManager geometries,
            LoadingManager loadingManager, InputManager inputManager)
            : base(parent, geometries)
        {
            this.loadingManager = loadingManager;
            this.inputManager = inputManager;

            loadingManager.Game.GameStatusChanged += onGameStatusChanged;
        }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);
            loadingManager.Update(args);
        }

        public override void Draw()
        {
            base.Draw();

            var txtGeo = Geometries.ConsoleFont;

            txtGeo.Color = Color.White;
            txtGeo.SizeCoefficient = Vector2.One;
            txtGeo.Height = 48;
            txtGeo.DrawString(new Vector2(16, 16), "Loading...");

            foreach (var (p, i) in loadingManager.Game.SortedPlayers.Indexed())
            {
                txtGeo.Color = p.ConnectionState == PlayerConnectionState.FinishedLoading ? Color.Lime : Color.Gold;
                txtGeo.DrawString(new Vector2(16, 144 + i * 64), p.Name);
            }
        }

        private void onGameStatusChanged(GameStatus t)
        {
            if (t != GameStatus.Playing)
                throw new Exception("Unexpected game status change.");
            startGame();
        }

        private void startGame()
        {
            loadingManager.IntegrateUI();
            Parent.AddScreenLayerOnTopOf(this,
                new GameUI(Parent, Geometries, loadingManager.Game, loadingManager.Network, inputManager));
            loadingManager.Game.GameStatusChanged -= onGameStatusChanged;
            Destroy();
        }
    }
}
