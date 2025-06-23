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
using Bearded.TD.Utilities;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using JetBrains.Annotations;

namespace Bearded.TD.UI.Controls;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
sealed class Lobby : UpdateableNavigationNode<LobbyManager>
{
    private LobbyManager lobbyManager = null!;
    private Logger logger = null!;

    private GameSettings.Builder gameSettings = null!;
    private ChatMessage? lastSeenChatMessage;

    private ImmutableHashSet<ModMetadata> enabledModsCache = ImmutableHashSet<ModMetadata>.Empty;
    public ImmutableArray<ModMetadata> AvailableMods { get; private set; } = ImmutableArray<ModMetadata>.Empty;

    private readonly Dictionary<Id<Player>, Binding<PlayerUIState>> playerUiStateLookup = new();
    private readonly List<IReadonlyBinding<PlayerUIState>> playerUiStates = [];
    public IList<IReadonlyBinding<PlayerUIState>> Players { get; }
    public ChatLog ChatLog => lobbyManager.Game.ChatLog;

    public bool CanChangeGameSettings => lobbyManager.CanChangeGameSettings;
    public ModAwareId GameMode => gameSettings.GameMode ?? throw new InvalidOperationException();
    public int LevelSize => gameSettings.LevelSize;
    public LevelGenerationMethod LevelGenerationMethod => gameSettings.LevelGenerationMethod;

    private bool gameModesNeedReloading = true;
    public ImmutableArray<ModAwareId> AvailableGameModes { get; private set; } = ImmutableArray<ModAwareId>.Empty;

    public bool CanToggleReady =>
        lobbyManager.Game.Content.IsFinishedLoading &&
        !enabledModsCache.IsEmpty &&
        GameMode.IsValid &&
        enabledModsCache.Contains(lobbyManager.Game.Content.FindMod(GameMode.ModId ?? ""));
    public ModLoadingProfiler LoadingProfiler => lobbyManager.Game.Content.LoadingProfiler;

    public event VoidEventHandler? LoadingUpdated;
    public event VoidEventHandler? PlayersChanged;
    public event VoidEventHandler? EnabledModsChanged;
    public event VoidEventHandler? AvailableGameModesChanged;
    public event VoidEventHandler? GameSettingsChanged;
    public event VoidEventHandler? ChatMessagesUpdated;

    public Lobby()
    {
        Players = playerUiStates.AsReadOnly();
    }

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

        AvailableMods = lobbyManager.Game.Content.AvailableMods.OrderBy(m => m.Name).ToImmutableArray();

        lobbyManager.Game.GameStatusChanged += onGameStatusChanged;
        lobbyManager.Game.PlayerAdded += onPlayerAdded;
        lobbyManager.Game.PlayerRemoved += onPlayerRemoved;
        lobbyManager.Game.GameSettingsChanged += onGameSettingsChanged;

        foreach (var p in lobbyManager.Game.Players)
        {
            onPlayerAdded(p);
        }
    }

    public override void Terminate()
    {
        base.Terminate();

        lobbyManager.Game.Content.CleanUpUnused();

        lobbyManager.Game.GameStatusChanged -= onGameStatusChanged;
        lobbyManager.Game.PlayerAdded -= onPlayerAdded;
        lobbyManager.Game.PlayerRemoved -= onPlayerRemoved;
        lobbyManager.Game.GameSettingsChanged -= onGameSettingsChanged;
    }

    public override void Update(UpdateEventArgs args)
    {
        lobbyManager.Update(args);
        updatePlayers();
        if (lobbyManager.Game.Status == GameStatus.Lobby)
        {
            LoadingUpdated?.Invoke();
        }

        if (!lobbyManager.Game.Content.EnabledMods.SetEquals(enabledModsCache))
        {
            enabledModsCache = lobbyManager.Game.Content.EnabledMods;
            onModsChanged();
        }

        if (gameModesNeedReloading && lobbyManager.Game.Content.IsFinishedLoading)
        {
            AvailableGameModes = lobbyManager.Game.Content.ListGameModes()
                .Select(gameMode => gameMode.Id)
                .ToImmutableArray();
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

    private void updatePlayers()
    {
        foreach (var player in lobbyManager.Game.Players)
        {
            var existingState = playerUiStateLookup[player.Id].Value;
            var currentState = PlayerUIState.FromPlayer(player);
            if (existingState != currentState)
            {
                playerUiStateLookup[player.Id].SetFromSource(currentState);
            }
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
        if (!lobbyManager.Game.Content.IsFinishedLoading)
        {
            return;
        }
        lobbyManager.ToggleReadyState();
    }

    public void OnBackToMenuButtonClicked()
    {
        if (!lobbyManager.Game.Content.IsFinishedLoading)
        {
            return;
        }
        lobbyManager.Game.Content.Dispose();
        lobbyManager.Close();
        Navigation!.Replace<MainMenu, Intent>(Intent.None, this);
    }

    public void OnSetModEnabled(ModMetadata mod, bool enabled)
    {
        var newEnabledMods = enabled
            ? lobbyManager.Game.Content.PreviewEnableMod(mod)
            : lobbyManager.Game.Content.PreviewDisableMod(mod);

        gameSettings.ActiveModIds.Clear();
        gameSettings.ActiveModIds.AddRange(newEnabledMods.Select(m => m.Id));

        lobbyManager.UpdateGameSettings(gameSettings.Build());
    }

    public bool IsModEnabled(ModMetadata mod) => lobbyManager.Game.Content.EnabledMods.Contains(mod);

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

    private void onPlayerAdded(Player player)
    {
        var binding = new Binding<PlayerUIState>();
        binding.SetFromSource(PlayerUIState.FromPlayer(player));
        playerUiStateLookup.Add(player.Id, binding);
        playerUiStates.Add(binding);
        onPlayersChanged();
    }

    private void onPlayerRemoved(Player player)
    {
        playerUiStateLookup.Remove(player.Id);
        playerUiStates.RemoveAll(b => b.Value.Id == player.Id);
        onPlayersChanged();
    }

    private void onPlayersChanged()
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

    public readonly record struct PlayerUIState(
        Id<Player> Id, string Name, PlayerConnectionState State, int LastKnownPing)
    {
        public static PlayerUIState FromPlayer(Player player) =>
            new(player.Id, player.Name, player.ConnectionState, player.LastKnownPing);
    }
}
