using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Players;
using Bearded.TD.Meta;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Controls;

sealed class Lobby : UpdateableNavigationNode<LobbyManager>
{
    private LobbyManager lobbyManager = null!;
    private Logger logger = null!;

    private GameSettings.Builder gameSettings = null!;
    private ChatMessage? lastSeenChatMessage;

    private ImmutableHashSet<ModMetadata> enabledModsCache = ImmutableHashSet<ModMetadata>.Empty;
    public ImmutableArray<ModMetadata> AvailableMods { get; private set; } = ImmutableArray<ModMetadata>.Empty;

    public IList<Player> Players => lobbyManager.Game.Players;
    public ChatLog ChatLog => lobbyManager.Game.ChatLog;

    public bool CanChangeGameSettings => lobbyManager.CanChangeGameSettings;
    public ModAwareId GameMode => gameSettings.GameMode ?? throw new InvalidOperationException();
    public int LevelSize => gameSettings.LevelSize;
    public LevelGenerationMethod LevelGenerationMethod => gameSettings.LevelGenerationMethod;

    private bool gameModesNeedReloading = true;
    public ImmutableArray<ModAwareId> AvailableGameModes { get; private set; } = ImmutableArray<ModAwareId>.Empty;

    public bool CanToggleReady =>
        lobbyManager.Game.ContentManager.IsFinishedLoading &&
        !enabledModsCache.IsEmpty &&
        GameMode.IsValid &&
        enabledModsCache.Contains(lobbyManager.Game.ContentManager.FindMod(GameMode.ModId ?? ""));
    public ModLoadingProfiler LoadingProfiler => lobbyManager.Game.ContentManager.LoadingProfiler;

    public event VoidEventHandler? LoadingUpdated;
    public event VoidEventHandler? PlayersChanged;
    public event VoidEventHandler? EnabledModsChanged;
    public event VoidEventHandler? AvailableGameModesChanged;
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
        gameSettings.GameMode ??= ModAwareId.ForDefaultMod("default");
        if (CanChangeGameSettings)
        {
            lobbyManager.UpdateGameSettings(gameSettings.Build());
        }

        AvailableMods = lobbyManager.Game.ContentManager.AvailableMods.OrderBy(m => m.Name).ToImmutableArray();

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

        if (!lobbyManager.Game.ContentManager.EnabledMods.SetEquals(enabledModsCache))
        {
            enabledModsCache = lobbyManager.Game.ContentManager.EnabledMods;
            onModsChanged();
        }

        if (gameModesNeedReloading && lobbyManager.Game.ContentManager.IsFinishedLoading)
        {
            AvailableGameModes = lobbyManager.Game.ContentManager.LoadedEnabledMods
                .SelectMany(m => m.Blueprints.GameModes.All)
                .Select(gameMode => gameMode.Id).ToImmutableArray();
            gameModesNeedReloading = false;
            AvailableGameModesChanged?.Invoke();
        }

        var chatMessages = lobbyManager.Game.ChatLog.Messages;
        if (chatMessages.Count > 0
            && (lastSeenChatMessage == null || chatMessages[^1] != lastSeenChatMessage))
        {
            lastSeenChatMessage = chatMessages[^1];
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
        Navigation!.Replace<MainMenu, Intent>(Intent.None, this);
    }

    public void OnSetModEnabled(ModMetadata mod, bool enabled)
    {
        var newEnabledMods = enabled
            ? lobbyManager.Game.ContentManager.PreviewEnableMod(mod)
            : lobbyManager.Game.ContentManager.PreviewDisableMod(mod);

        gameSettings.ActiveModIds.Clear();
        gameSettings.ActiveModIds.AddRange(newEnabledMods.Select(m => m.Id));

        lobbyManager.UpdateGameSettings(gameSettings.Build());
    }

    public bool IsModEnabled(ModMetadata mod) => lobbyManager.Game.ContentManager.EnabledMods.Contains(mod);

    public void OnSetGameMode(ModAwareId gameMode)
    {
        gameSettings.GameMode = gameMode;
        lobbyManager.UpdateGameSettings(gameSettings.Build());
    }

    public void OnSetLevelSize(int size)
    {
        gameSettings.LevelSize = size;
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
            UserSettings.Instance.LastGameSettings = new GameSettings.Builder(gameSettings);
            UserSettings.RaiseSettingsChanged();
            UserSettings.Save(logger);
        }
        Navigation!.Replace<LoadingScreen, LoadingManager>(lobbyManager.GetLoadingManager(), this);
    }

    private void onPlayersChanged(Player player)
    {
        PlayersChanged?.Invoke();
    }

    private void onModsChanged()
    {
        gameModesNeedReloading = true;
        EnabledModsChanged?.Invoke();
    }

    private void onGameSettingsChanged(IGameSettings newGameSettings)
    {
        gameSettings = new GameSettings.Builder(newGameSettings);
        GameSettingsChanged?.Invoke();
    }
}
