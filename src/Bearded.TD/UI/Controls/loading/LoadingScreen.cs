using System;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.UI.Navigation;

namespace Bearded.TD.UI.Controls
{
    sealed class LoadingScreen : UpdateableNavigationNode<LoadingManager>
    {
        private LoadingManager loadingManager;

        protected override void Initialize(DependencyResolver dependencies, LoadingManager loadingManager)
        {
            base.Initialize(dependencies, loadingManager);

            this.loadingManager = loadingManager;
            loadingManager.Game.GameStatusChanged += onGameStatusChanged;
        }

        public override void Terminate()
        {
            base.Terminate();

            loadingManager.Game.GameStatusChanged -= onGameStatusChanged;
        }

        public override void Update(UpdateEventArgs args)
        {
            loadingManager.Update(args);
        }

        private void onGameStatusChanged(GameStatus gameStatus)
        {
            if (gameStatus != GameStatus.Playing) throw new Exception("Unexpected game status change.");
            startGame();
        }

        private void startGame()
        {
            loadingManager.IntegrateUI();
            Navigation.Replace<GameWorld, (GameInstance, GameRunner)>(
                (loadingManager.Game, new GameRunner(loadingManager.Game, loadingManager.Network)), this);
        }
    }
}
