using System;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.UI.Navigation;

namespace Bearded.TD.UI.Controls
{
    sealed class Lobby : UpdateableNavigationNode<LobbyManager>
    {
        private LobbyManager lobbyManager;

        protected override void Initialize(DependencyResolver dependencies, LobbyManager lobbyManager)
        {
            base.Initialize(dependencies, lobbyManager);
            this.lobbyManager = lobbyManager;
            lobbyManager.Game.GameStatusChanged += onGameStatusChanged;
        }

        public override void Terminate()
        {
            base.Terminate();
            lobbyManager.Game.GameStatusChanged -= onGameStatusChanged;
        }

        public override void Update(UpdateEventArgs args)
        {
            lobbyManager.Update(args);
        }

        public void OnToggleReadyButtonClicked()
        {
            lobbyManager.ToggleReadyState();
        }

        public void OnBackToMenuButtonClicked()
        {
            lobbyManager.Close();
            Navigation.Replace<MainMenu>(this);
        }

        private void onGameStatusChanged(GameStatus gameStatus)
        {
            if (gameStatus != GameStatus.Loading) throw new Exception("Unexpected game status change.");
            beginLoading();
        }

        private void beginLoading()
        {
            // TODO: go to lobby screen
        }
    }
}
