using System;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Audio;
using Bearded.TD.Commands;
using Bearded.TD.Game.Camera;
using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Input;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Game;

sealed class GameInstance
{
    public GameContent Content { get; }
    public Player Me { get; }
    public IRequestDispatcher<Player, GameInstance> RequestDispatcher { get; }
    public GameMeta Meta { get; }

    public ChatLog ChatLog { get; } = new();
    public PlayerCursors PlayerCursors { get; }
    public IdManager Ids { get; }
    public LevelDebugMetadata LevelDebugMetadata { get; }

    public GameSettings GameSettings { get; private set; } = new GameSettings.Builder().Build();

    private GameState? state;
    public GameState State => state!;

    private GameCamera? camera;
    public GameCamera Camera => camera!;

    private PlayerInput? playerInput;
    public PlayerInput PlayerInput => playerInput!;

    private GameCameraController? cameraController;
    public GameCameraController CameraController => cameraController!;

    private SelectionManager? selectionManager;
    public SelectionManager SelectionManager => selectionManager!;

    public ActiveOverlays Overlays { get; } = new();

    private readonly IdCollection<Player> players = new();
    public ReadOnlyCollection<Player> Players => players.AsReadOnly;
    public ReadOnlyCollection<Player> SortedPlayers
    {
        get
        {
            var sortedPlayers = Players.ToList();
            sortedPlayers.Sort((p1, p2) => p1.Id.Value.CompareTo(p2.Id.Value));
            return sortedPlayers.AsReadOnly();
        }
    }

    private Blueprints? blueprints;
    public Blueprints Blueprints => blueprints!;

    private GameStatus status = GameStatus.Lobby;
    public GameStatus Status
    {
        get => status;
        private set
        {
            status = value;
            GameStatusChanged?.Invoke(status);
        }
    }

    public event GenericEventHandler<GameStatus>? GameStatusChanged;
    public event GenericEventHandler<GameSettings>? GameSettingsChanged;
    public event GenericEventHandler<Player>? PlayerAdded;
    public event GenericEventHandler<Player>? PlayerRemoved;

    private readonly PlayerManager? playerManager;

    public GameInstance(IGameContext context,
        GameContent content,
        Player me,
        IdManager ids,
        RenderContext renderContext)
    {
        RequestDispatcher = context.RequestDispatcher;
        context.DataMessageHandlerInitializer(this);
        Content = content;
        Me = me;
        Ids = ids;
        LevelDebugMetadata = new LevelDebugMetadata();

        AddPlayer(me);

        PlayerCursors = new PlayerCursors(this);
        playerManager = context.PlayerManagerFactory(this);
        Meta = new GameMeta(
            context.Logger,
            context.Dispatcher,
            context.GameSynchronizer,
            ids,
            // TODO: oh so bad and leaky, this really shouldn't be here, but then again this whole class is bad
            new DrawableRenderers(renderContext.Settings),
            SoundScape.WithChannelCount(Constants.Audio.SoundEffectChannelCount));
    }

    public void SetGameSettings(GameSettings gameSettings)
    {
        if (Status != GameStatus.Lobby)
            throw new InvalidOperationException("Can only change game settings in the lobby.");
        GameSettings = gameSettings;
        Content.SetEnabledModsById(gameSettings.ActiveModIds);
        GameSettingsChanged?.Invoke(gameSettings);
    }

    public void AddPlayer(Player player)
    {
        players.Add(player);
        PlayerAdded?.Invoke(player);
    }

    public void RemovePlayer(Player player)
    {
        players.Remove(player);
        PlayerRemoved?.Invoke(player);
    }

    public Player PlayerFor(Id<Player> id) => players[id];

    public Player PlayerFor(NetIncomingMessage msg) => playerManager!.GetSender(msg);

    public void UpdatePlayers(UpdateEventArgs args)
    {
        playerManager?.Update(args);
    }

    public void SetLoading()
    {
        if (Status != GameStatus.Lobby)
        {
            throw new InvalidOperationException("Can only initialize loading from the lobby state.");
        }

        Status = GameStatus.Loading;
        setAllPlayerConnectionStates(PlayerConnectionState.AwaitingLoadingData);
    }

    public void Start()
    {
        if (Status != GameStatus.Loading)
        {
            throw new InvalidOperationException("Can only start the game from the loading state.");
        }

        Status = GameStatus.Playing;
        setAllPlayerConnectionStates(PlayerConnectionState.Playing);
        Meta.Events.Send(new GameStarted());
    }

    private void gatherBlueprints()
    {
        if (Status != GameStatus.Loading)
        {
            throw new InvalidOperationException("Cannot set blueprints if the game is not loading.");
        }
        if (blueprints != null)
        {
            throw new InvalidOperationException("Cannot override the blueprints once set.");
        }

        blueprints = Content.CreateBlueprints();
        Meta.SetBlueprints(blueprints);
    }

    public void InitializeState(GameState initialState)
    {
        if (state != null)
            throw new InvalidOperationException("Cannot override the game state once set.");

        gatherBlueprints();
        Meta.SetState(initialState);

        state = initialState;
    }

    public void PrepareUI()
    {
        if (camera != null)
        {
            throw new InvalidOperationException("Cannot override the camera once set.");
        }
        if (state == null)
        {
            throw new InvalidOperationException("UI should be integrated after the game state is initialised.");
        }

        camera = new PerspectiveGameCamera();
        cameraController = new GameCameraController(Camera, State.Level.Radius);
        selectionManager = new SelectionManager();
        playerInput = new PlayerInput(this);
        state.Meta.SetUIDrawState(playerInput.UIDrawState);
    }

    private void setAllPlayerConnectionStates(PlayerConnectionState playerConnectionState)
    {
        foreach (var p in Players)
        {
            p.ConnectionState = playerConnectionState;
        }
    }
}
