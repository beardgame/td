using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Players;
using Bearded.TD.Meta;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Controls
{
    sealed class Lobby : UpdateableNavigationNode<LobbyManager>
    {
        private LobbyManager lobbyManager;
        private Logger logger;

        private GameSettings.Builder gameSettings;

        public IList<Player> Players => lobbyManager.Game.Players;

        public bool CanChangeGameSettings => lobbyManager.CanChangeGameSettings;
        public int LevelSize => gameSettings.LevelSize;
        public WorkerDistributionMethod WorkerDistributionMethod => gameSettings.WorkerDistributionMethod;
        public LevelGenerationMethod LevelGenerationMethod => gameSettings.LevelGenerationMethod;

        public bool CanToggleReady => lobbyManager.Game.ContentManager.IsFinishedLoading;
        public ModLoadingProfiler LoadingProfiler => lobbyManager.Game.ContentManager.LoadingProfiler;

        public event VoidEventHandler PlayersChanged;
        public event VoidEventHandler LoadingUpdated;
        public event VoidEventHandler GameSettingsChanged;

        protected override void Initialize(DependencyResolver dependencies, LobbyManager lobbyManager)
        {
            base.Initialize(dependencies, lobbyManager);

            this.lobbyManager = lobbyManager;
            logger = dependencies.Resolve<Logger>();

            gameSettings = CanChangeGameSettings && UserSettings.Instance.LastGameSettings != null
                ? new GameSettings.Builder(UserSettings.Instance.LastGameSettings)
                : new GameSettings.Builder();
            if (CanChangeGameSettings)
            {
                if (UserSettings.Instance.Misc.MapGenSeed.HasValue)
                {
                    gameSettings.Seed = UserSettings.Instance.Misc.MapGenSeed.Value;
                }
                lobbyManager.UpdateGameSettings(gameSettings.Build());
            }

            lobbyManager.Game.GameStatusChanged += onGameStatusChanged;
            lobbyManager.Game.PlayerAdded += onPlayersChanged;
            lobbyManager.Game.PlayerRemoved += onPlayersChanged;
            lobbyManager.Game.GameSettingsChanged += onGameSettingsChanged;
        }

        public override void Terminate()
        {
            base.Terminate();

            lobbyManager.Game.ContentManager.CleanUp();

            lobbyManager.Game.GameStatusChanged -= onGameStatusChanged;
            lobbyManager.Game.PlayerAdded -= onPlayersChanged;
            lobbyManager.Game.PlayerRemoved -= onPlayersChanged;
        }

        public override void Update(UpdateEventArgs args)
        {
            lobbyManager.Update(args);
            if (lobbyManager.Game.Status == GameStatus.Lobby)
            {
                LoadingUpdated?.Invoke();
            }
        }

        public void OnToggleReadyButtonClicked()
        {
            if (!lobbyManager.Game.ContentManager.IsFinishedLoading)
            {
                return;
            }
            lobbyManager.ToggleReadyState();
        }

        public void OnBackToMenuButtonClicked()
        {
            if (!lobbyManager.Game.ContentManager.IsFinishedLoading)
            {
                return;
            }
            lobbyManager.Game.ContentManager.CleanUpAll();
            lobbyManager.Close();
            Navigation.Replace<MainMenu>(this);
        }

        public void OnSetLevelSize(int size)
        {
            gameSettings.LevelSize = size;
            lobbyManager.UpdateGameSettings(gameSettings.Build());
        }

        public void OnCycleWorkerDistributionMethod()
        {
            gameSettings.WorkerDistributionMethod = gameSettings.WorkerDistributionMethod + 1;
            if ((byte) gameSettings.WorkerDistributionMethod
                >= Enum.GetValues(WorkerDistributionMethod.GetType()).Length)
            {
                gameSettings.WorkerDistributionMethod = 0;
            }
            lobbyManager.UpdateGameSettings(gameSettings.Build());
        }

        public void OnCycleLevelGenerationMethod()
        {
            gameSettings.LevelGenerationMethod = gameSettings.LevelGenerationMethod + 1;
            if ((byte) gameSettings.LevelGenerationMethod >= Enum.GetValues(LevelGenerationMethod.GetType()).Length)
            {
                gameSettings.LevelGenerationMethod = 0;
            }
            lobbyManager.UpdateGameSettings(gameSettings.Build());
        }

        private void onGameStatusChanged(GameStatus gameStatus)
        {
            if (gameStatus != GameStatus.Loading) throw new Exception("Unexpected game status change.");
            if (CanChangeGameSettings)
            {
                UserSettings.Instance.LastGameSettings = new GameSettings.Builder(gameSettings) {Seed = 0};
                UserSettings.RaiseSettingsChanged();
                UserSettings.Save(logger);
            }
            Navigation.Replace<LoadingScreen, LoadingManager>(lobbyManager.GetLoadingManager(), this);
        }

        private void onPlayersChanged(Player player)
        {
            PlayersChanged?.Invoke();
        }

        private void onGameSettingsChanged(IGameSettings newGameSettings)
        {
            gameSettings = new GameSettings.Builder(newGameSettings, includeRandomAttributes: true);
            GameSettingsChanged?.Invoke();
        }
    }
}
