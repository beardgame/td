﻿using System;
using System.Collections.ObjectModel;
using Bearded.TD.Commands;
using Bearded.TD.Game.Blueprints;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.UI;
using Bearded.TD.Networking;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    class GameInstance
    {
        public Player Me { get; }
        public IRequestDispatcher RequestDispatcher { get; }
        public IDataMessageHandler DataMessageHandler { get; }
        public IGameController Controller { get; }

        public GameMeta Meta { get; }
        
        public ChatLog ChatLog { get; } = new ChatLog();
        public IdManager Ids { get; }

        public GameState State { get; private set; }
        public GameCamera Camera { get; private set; }
        public CursorState Cursor { get; private set; }

        private readonly IdCollection<Player> players = new IdCollection<Player>();
        public ReadOnlyCollection<Player> Players => players.AsReadOnly;

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

        public GameInstance(IGameContext context, Player me, IdManager ids)
        {
            Me = me;
            RequestDispatcher = context.RequestDispatcher;
            DataMessageHandler = context.DataMessageHandlerFactory(this);
            Controller = context.GameSimulatorFactory(this);
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
