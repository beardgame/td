using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.UI.Input;
using Bearded.TD.UI.Model;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    class GameInstance
    {
        public ContentManager ContentManager { get; }
        public Player Me { get; }
        public IRequestDispatcher RequestDispatcher { get; }
        public IDataMessageHandler DataMessageHandler { get; }
        public IGameController Controller { get; }
        public IEnumerable<Mod> Mods { get; }
        public GameMeta Meta { get; }
        
        public ChatLog ChatLog { get; } = new ChatLog();
        public IdManager Ids { get; }

        public GameState State { get; private set; }
        public PlayerInput PlayerInput { get; private set; }
        public GameCamera Camera { get; private set; }
        public SelectionManager SelectionManager { get; private set; }

        private readonly IdCollection<Player> players = new IdCollection<Player>();
        public ReadOnlyCollection<Player> Players => players.AsReadOnly;

        public Blueprints Blueprints { get; } = new Blueprints();

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

        public GameInstance(IGameContext context, ContentManager contentManager, Player me, IdManager ids)
        {
            RequestDispatcher = context.RequestDispatcher;
            DataMessageHandler = context.DataMessageHandlerFactory(this);
            Controller = context.GameSimulatorFactory(this);
            Mods = context.Mods;
            ContentManager = contentManager;
            Me = me;
            Ids = ids;

            AddPlayer(me);

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

        public void SetLoading()
        {
            if (Status != GameStatus.Lobby)
                throw new Exception();
            Status = GameStatus.Loading;
            foreach (var p in Players)
                p.ConnectionState = PlayerConnectionState.DownloadingMods;
        }

        public void Start()
        {
            if (Status != GameStatus.Loading)
                throw new Exception();
            Status = GameStatus.Playing;
            foreach (var p in Players)
                p.ConnectionState = PlayerConnectionState.Playing;
        }

        public void IntegrateUI(GameCamera camera)
        {
            if (Camera != null)
                throw new Exception();
            Camera = camera;
            SelectionManager = new SelectionManager();
            PlayerInput = new PlayerInput(this);
        }

        public void InitialiseState(GameState state)
        {
            if (State != null)
                throw new Exception();
            State = state;
        }
    }
}
