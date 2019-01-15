using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.UI.Navigation;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class Lobby : UpdateableNavigationNode<LobbyManager>
    {
        private LobbyManager lobbyManager;
        public IList<Player> Players => lobbyManager.Game.Players;

        private GameSettings.Builder gameSettings;

        // todo: replace by proper game settings type
        // todo: save last game settings in user settings (server only? optionally?)
        // todo: sync settings from server to client when changed
        // todo: make ui on client read-only
        public int LevelSize => gameSettings.LevelSize;

        public event VoidEventHandler PlayersChanged;

        protected override void Initialize(DependencyResolver dependencies, LobbyManager lobbyManager)
        {
            base.Initialize(dependencies, lobbyManager);

            this.lobbyManager = lobbyManager;
            gameSettings = new GameSettings.Builder();
            lobbyManager.Game.GameStatusChanged += onGameStatusChanged;
            lobbyManager.Game.PlayerAdded += onPlayersChanged;
            lobbyManager.Game.PlayerRemoved += onPlayersChanged;
        }

        public override void Terminate()
        {
            base.Terminate();

            lobbyManager.Game.GameStatusChanged -= onGameStatusChanged;
            lobbyManager.Game.PlayerAdded -= onPlayersChanged;
            lobbyManager.Game.PlayerRemoved -= onPlayersChanged;
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

        public void OnSetLevelSize(int size)
        {
            gameSettings.LevelSize = size;
        }

        private void onGameStatusChanged(GameStatus gameStatus)
        {
            if (gameStatus != GameStatus.Loading) throw new Exception("Unexpected game status change.");
            Navigation.Replace<LoadingScreen, LoadingManager>(lobbyManager.GetLoadingManager(gameSettings.Build()), this);
        }

        private void onPlayersChanged(Player player)
        {
            PlayersChanged?.Invoke();
        }
    }
}
