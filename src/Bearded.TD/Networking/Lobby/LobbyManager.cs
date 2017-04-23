﻿using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.UI;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;

namespace Bearded.TD.Networking.Lobby
{
    abstract class LobbyManager
    {
        private readonly IDispatcher dispatcher;
        public Logger Logger { get; }
        public GameInstance Game { get; }

        public abstract bool GameStarted { get; }
        public abstract IReadOnlyList<LobbyPlayer> Players { get; }

        protected LobbyManager(Logger logger,
            (IRequestDispatcher request, IDispatcher master) dispatchers)
            : this(logger, dispatchers.master)
        {
            var player = new Player(new Utilities.Id<Player>(), "The host", Color.Gray);
            Game = new GameInstance(player, dispatchers.request);
        }

        protected LobbyManager(Logger logger, Player player,
            (IRequestDispatcher request, IDispatcher master) dispatchers)
            : this(logger, dispatchers.master)
        {
            Game = new GameInstance(player, dispatchers.request);
        }

        private LobbyManager(Logger logger, IDispatcher dispatcher)
        {
            Logger = logger;
            this.dispatcher = dispatcher;
        }

        public abstract void Update(UpdateEventArgs args);

        public abstract void ToggleReadyState();

        public GameInstance GetStartedInstance(InputManager inputManager)
        {
            var meta = new GameMeta(Logger, dispatcher, Game.Ids);
            var gameState = GameStateBuilder.Generate(meta, new DefaultTilemapGenerator(Logger));
            var camera = new GameCamera(inputManager, meta, gameState.Level.Tilemap.Radius);

            Game.Start(gameState, camera);

            return Game;
        }
    }
}