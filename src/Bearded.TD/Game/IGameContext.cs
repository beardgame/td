using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game
{
    interface IGameContext
    {
        Logger Logger { get; }
        IDispatcher<GameInstance> Dispatcher { get; }
        IRequestDispatcher<GameInstance> RequestDispatcher { get; }
        IGameSynchronizer GameSynchronizer { get; }
        Func<GameInstance, PlayerManager> PlayerManagerFactory { get; }
        Action<GameInstance> DataMessageHandlerInitializer { get; }
        Func<GameInstance, IGameController> GameSimulatorFactory { get; }
    }

    sealed class ServerGameContext : IGameContext
    {
        public Logger Logger { get; }
        public IDispatcher<GameInstance> Dispatcher { get; }
        public IRequestDispatcher<GameInstance> RequestDispatcher { get; }
        public IGameSynchronizer GameSynchronizer { get; }
        public Func<GameInstance, PlayerManager> PlayerManagerFactory { get; }
        public Action<GameInstance> DataMessageHandlerInitializer { get; }
        public Func<GameInstance, IGameController> GameSimulatorFactory { get; }

        public ServerGameContext(ServerNetworkInterface network, Logger logger)
        {
            Logger = logger;

            var commandDispatcher = new ServerCommandDispatcher<GameInstance>(new DefaultCommandExecutor(), network);
            RequestDispatcher = new ServerRequestDispatcher<GameInstance>(commandDispatcher, logger);
            Dispatcher = new ServerDispatcher<GameInstance>(commandDispatcher);
            GameSynchronizer = new ServerGameSynchronizer(commandDispatcher, logger);
            PlayerManagerFactory = game =>
            {
                var playerManager = new PlayerManager(game, network, Dispatcher);
                network.RegisterMessageHandler(playerManager);
                return playerManager;
            };
            DataMessageHandlerInitializer = game => network.RegisterMessageHandler(new ServerDataMessageHandler(game, logger));
            GameSimulatorFactory = game => new GameController(game);
        }
    }

    sealed class ClientGameContext : IGameContext
    {
        public Logger Logger { get; }
        public IDispatcher<GameInstance> Dispatcher { get; }
        public IRequestDispatcher<GameInstance> RequestDispatcher { get; }
        public IGameSynchronizer GameSynchronizer { get; }
        public Func<GameInstance, PlayerManager> PlayerManagerFactory { get; }
        public Action<GameInstance> DataMessageHandlerInitializer { get; }
        public Func<GameInstance, IGameController> GameSimulatorFactory { get; }

        public ClientGameContext(ClientNetworkInterface network, Logger logger)
        {
            Logger = logger;

            RequestDispatcher = new ClientRequestDispatcher<GameInstance>(network, logger);
            Dispatcher = new ClientDispatcher<GameInstance>();
            GameSynchronizer = new ClientGameSynchronizer();
            PlayerManagerFactory = game => null;
            DataMessageHandlerInitializer = game => network.RegisterMessageHandler(new ClientDataMessageHandler(game, logger));
            GameSimulatorFactory = game => new DummyGameController();
        }
    }
}
