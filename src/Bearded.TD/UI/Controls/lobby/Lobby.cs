using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Meta;
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
        private ChatMessage? lastSeenChatMessage;

        private ImmutableHashSet<ModMetadata> enabledMods = ImmutableHashSet<ModMetadata>.Empty;
        public ImmutableList<ModMetadata> AvailableMods { get; private set; } = ImmutableList<ModMetadata>.Empty;

        public IList<Player> Players => lobbyManager.Game.Players;
        public ChatLog ChatLog => lobbyManager.Game.ChatLog;

        public bool CanChangeGameSettings => lobbyManager.CanChangeGameSettings;
        public int LevelSize => gameSettings.LevelSize;
        public WorkerDistributionMethod WorkerDistributionMethod => gameSettings.WorkerDistributionMethod;
        public LevelGenerationMethod LevelGenerationMethod => gameSettings.LevelGenerationMethod;

        public bool CanToggleReady => lobbyManager.Game.ContentManager.IsFinishedLoading;
        public ModLoadingProfiler LoadingProfiler => lobbyManager.Game.ContentManager.LoadingProfiler;

        public event VoidEventHandler? LoadingUpdated;
        public event VoidEventHandler? PlayersChanged;
        public event VoidEventHandler? ModsChanged;
        public event VoidEventHandler? GameSettingsChanged;
        public event VoidEventHandler? ChatMessagesUpdated;

        protected override void Initialize(DependencyResolver dependencies, LobbyManager lobbyManager)
        {
            base.Initialize(dependencies, lobbyManager);

            this.lobbyManager = lobbyManager;
            logger = dependencies.Resolve<Logger>();

            gameSettings = CanChangeGameSettings
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

            AvailableMods = lobbyManager.Game.ContentManager.AvailableMods.OrderBy(m => m.Name).ToImmutableList();

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
            lobbyManager.Game.GameSettingsChanged -= onGameSettingsChanged;
        }

        public override void Update(UpdateEventArgs args)
        {
            lobbyManager.Update(args);
            if (lobbyManager.Game.Status == GameStatus.Lobby)
            {
                LoadingUpdated?.Invoke();
            }

            if (!lobbyManager.Game.ContentManager.EnabledMods.SetEquals(enabledMods))
            {
                enabledMods = lobbyManager.Game.ContentManager.EnabledMods;
                onModsChanged();
            }

            var chatMessages = lobbyManager.Game.ChatLog.Messages;
            if (chatMessages.Count > 0
                && (lastSeenChatMessage == null || chatMessages[chatMessages.Count - 1] != lastSeenChatMessage))
            {
                lastSeenChatMessage = chatMessages[chatMessages.Count - 1];
                ChatMessagesUpdated?.Invoke();
            }
        }

        public void OnSendChatMessage(string value)
        {
            lobbyManager.Game.RequestDispatcher.Dispatch(
                lobbyManager.Game.Me,
                SendChatMessage.Request(lobbyManager.Game, lobbyManager.Game.Me, value));
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

        public void OnSetModEnabled(ModMetadata mod, bool enabled)
        {
            lobbyManager.UpdateModEnabled(mod, enabled);
        }

        public bool IsModEnabled(ModMetadata mod) => lobbyManager.Game.ContentManager.EnabledMods.Contains(mod);

        public void OnSetLevelSize(int size)
        {
            gameSettings.LevelSize = size;
            lobbyManager.UpdateGameSettings(gameSettings.Build());
        }

        public void OnSetWorkerDistributionMethod(WorkerDistributionMethod method)
        {
            gameSettings.WorkerDistributionMethod = method;
            lobbyManager.UpdateGameSettings(gameSettings.Build());
        }

        public void OnSetLevelGenerationMethod(LevelGenerationMethod method)
        {
            gameSettings.LevelGenerationMethod = method;
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

        private void onModsChanged()
        {
            ModsChanged?.Invoke();
        }

        private void onGameSettingsChanged(IGameSettings newGameSettings)
        {
            gameSettings = new GameSettings.Builder(newGameSettings, includeRandomAttributes: true);
            GameSettingsChanged?.Invoke();
        }
    }
}
