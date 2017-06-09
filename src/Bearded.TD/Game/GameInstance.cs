using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Commands;
using Bearded.TD.Game.Blueprints;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.UI;
using Bearded.TD.Networking;
using Bearded.TD.Utilities;
using Bearded.Utilities;

using DataMessageHandlerFactory = System.Func<Bearded.TD.Game.GameInstance, Bearded.TD.Networking.IDataMessageHandler>;

namespace Bearded.TD.Game
{
    class GameInstance
    {
        public Player Me { get; }
        public IRequestDispatcher RequestDispatcher { get; }
        public IDataMessageHandler DataMessageHandler { get; }

        public GameMeta Meta { get; }
        
        public ChatLog ChatLog { get; } = new ChatLog();
        public IdManager Ids { get; }

        public GameState State { get; private set; }
        public GameCamera Camera { get; private set; }
        public CursorState Cursor { get; private set; }

        private readonly IdCollection<Player> players = new IdCollection<Player>();
        public ReadOnlyCollection<Player> Players => players.AsReadOnly;

        private readonly IdCollection<Faction> factions = new IdCollection<Faction>();
        public ReadOnlyCollection<Faction> Factions => factions.AsReadOnly;

        public BlueprintManager Blueprints { get; } = new BlueprintManager();

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

        public GameInstance(
            Player me, IRequestDispatcher requestDispatcher,
            IDispatcher dispatcher, Logger logger,
            DataMessageHandlerFactory dataMessageHandlerFactory, IdManager ids)
        {
            Me = me;
            RequestDispatcher = requestDispatcher;
            DataMessageHandler = dataMessageHandlerFactory(this);
            Ids = ids;

            AddPlayer(me);

            Meta = new GameMeta(logger, dispatcher, ids);
        }

        public void AddPlayer(Player player)
        {
            players.Add(player);
        }

        public void RemovePlayer(Player player)
        {
            players.Remove(player);
        }

        public Faction FactionFor(Utilities.Id<Faction> id) => factions[id];

        public void AddFaction(Faction faction)
        {
            factions.Add(faction);
        }

        public void RemoveFaction(Faction faction)
        {
            factions.Remove(faction);
        }

        public Player PlayerFor(Utilities.Id<Player> id) => players[id];

        public void SetLoading()
        {
            if (Status != GameStatus.Lobby)
                throw new Exception();
            Status = GameStatus.Loading;
            foreach (var p in Players)
                p.ConnectionState = PlayerConnectionState.AwaitingLoadingData;
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
            Cursor = new CursorState(this);
        }

        public void InitialiseState(GameState state)
        {
            if (State != null)
                throw new Exception();
            State = state;
        }
    }
}
