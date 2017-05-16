using System;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Loading;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Networking.Lobby
{
    abstract class LobbyManager
    {
        public Logger Logger { get; }
        public GameInstance Game { get; }
        protected IDispatcher Dispatcher { get; }

        protected LobbyManager(
            Logger logger,
            (IRequestDispatcher request, IDispatcher master) dispatchers,
            Func<GameInstance, IDataMessageHandler> dataMessageHandlerFactory)
            : this(logger, dispatchers.master)
        {
            var ids = new IdManager();
            var player = new Player(ids.GetNext<Player>(), "The host", Color.Red)
            {
                ConnectionState = PlayerConnectionState.Waiting
            };
            Game = new GameInstance(player, dispatchers.request, dispatchers.master, logger, dataMessageHandlerFactory, ids);
        }

        protected LobbyManager(
            Logger logger,
            Player player,
            (IRequestDispatcher request, IDispatcher master) dispatchers,
            Func<GameInstance, IDataMessageHandler> dataMessageHandlerFactory)
            : this(logger, dispatchers.master)
        {
            Game = new GameInstance(player, dispatchers.request, dispatchers.master, logger, dataMessageHandlerFactory, null);
        }

        private LobbyManager(Logger logger, IDispatcher dispatcher)
        {
            Logger = logger;
            Dispatcher = dispatcher;
        }

        public void ToggleReadyState()
        {
            var connectionState =
                    Game.Me.ConnectionState == PlayerConnectionState.Ready
                        ? PlayerConnectionState.Waiting
                        : PlayerConnectionState.Ready;

            Game.RequestDispatcher.Dispatch(ChangePlayerState.Request(Game.Me, connectionState));
        }

        public abstract void Update(UpdateEventArgs args);
        public abstract LoadingManager GetLoadingManager();
    }
}
