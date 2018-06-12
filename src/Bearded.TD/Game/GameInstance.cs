using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Players;
using Bearded.TD.Mods;
using Bearded.TD.UI.Input;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    class GameInstance
    {
        public ContentManager ContentManager { get; }
        public Player Me { get; }
        public IRequestDispatcher<GameInstance> RequestDispatcher { get; }
        public IGameController Controller { get; }
        public GameMeta Meta { get; }
        
        public ChatLog ChatLog { get; } = new ChatLog();
        public IdManager Ids { get; }

        public GameState State { get; private set; }
        public PlayerInput PlayerInput { get; private set; }
        public GameCamera Camera { get; private set; }
        public SelectionManager SelectionManager { get; private set; }

        private readonly IdCollection<Player> players = new IdCollection<Player>();
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

        public Blueprints Blueprints { get; private set; }

        private GameStatus status = GameStatus.Lobby;
        public GameStatus Status
        {
            get => status;
            set
            {
                status = value;
                GameStatusChanged?.Invoke(status);
            }
        }
        public event GenericEventHandler<GameStatus> GameStatusChanged;
        
        private readonly PlayerManager playerManager;

        public GameInstance(IGameContext context, ContentManager contentManager, Player me, IdManager ids)
        {
            RequestDispatcher = context.RequestDispatcher;
            context.DataMessageHandlerInitializer(this);
            Controller = context.GameSimulatorFactory(this);
            ContentManager = contentManager;
            Me = me;
            Ids = ids;

            AddPlayer(me);

            playerManager = context.PlayerManagerFactory(this);
            Meta = new GameMeta(context.Logger, context.Dispatcher, context.GameSynchronizer, ids);
        }

        public void AddPlayer(Player player)
        {
            players.Add(player);
        }

        public void RemovePlayer(Player player)
        {
            players.Remove(player);
        }

        public Player PlayerFor(Id<Player> id) => players[id];

        public void UpdatePlayers(UpdateEventArgs args)
        {
            playerManager?.Update(args);
        }

        public void SetBlueprints(Blueprints blueprints)
        {
            if (Status != GameStatus.Loading)
                throw new InvalidOperationException("Cannot set blueprints if the game is not loading.");
            if (Blueprints != null)
                throw new InvalidOperationException("Cannot override the blueprints once set.");

            Blueprints = blueprints;
        }

        public void SetLoading()
        {
            if (Status != GameStatus.Lobby)
                throw new InvalidOperationException("Can only initialize loading from the lobby state.");
            Status = GameStatus.Loading;
            foreach (var p in Players)
                p.ConnectionState = PlayerConnectionState.LoadingMods;
        }

        public void Start()
        {
            if (Status != GameStatus.Loading)
                throw new InvalidOperationException("Can only start the game from the loading state.");
            if (Blueprints == null)
                throw new InvalidOperationException("Cannot start game before blueprints are set.");
            Status = GameStatus.Playing;
            foreach (var p in Players)
                p.ConnectionState = PlayerConnectionState.Playing;
        }

        public void IntegrateUI(GameCamera camera)
        {
            if (Camera != null)
                throw new InvalidOperationException("Cannot override the camera once set.");
            Camera = camera;
            SelectionManager = new SelectionManager();
            PlayerInput = new PlayerInput(this);
        }

        public void InitialiseState(GameState state)
        {
            if (State != null)
                throw new InvalidOperationException("Cannot override the gamestate once set.");
            State = state;
        }
    }
}
