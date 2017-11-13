using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game
{
    interface IGameContext
    {
        Logger Logger { get; }
        IDispatcher<GameInstance> Dispatcher { get; }
        IRequestDispatcher<GameInstance, Player> RequestDispatcher { get; }
        IGameSynchronizer GameSynchronizer { get; }
        Func<GameInstance, IDataMessageHandler<GameInstance, Player>> DataMessageHandlerFactory { get; }
        Func<GameInstance, IGameController> GameSimulatorFactory { get; }
    }

    sealed class ServerGameContext : IGameContext
    {
        public Logger Logger { get; }
        public IDispatcher<GameInstance> Dispatcher { get; }
        public IRequestDispatcher<GameInstance, Player> RequestDispatcher { get; }
        public IGameSynchronizer GameSynchronizer { get; }
        public Func<GameInstance, IDataMessageHandler<GameInstance, Player>> DataMessageHandlerFactory { get; }
        public Func<GameInstance, IGameController> GameSimulatorFactory { get; }

        public ServerGameContext(ServerNetworkInterface<Player> network, Logger logger)
        {
            Logger = logger;

            var commandDispatcher
                = new ServerCommandDispatcher<GameInstance, Player>(new DefaultCommandExecutor<GameInstance>(), network);
            RequestDispatcher = new ServerRequestDispatcher<GameInstance, Player>(commandDispatcher, logger);
            Dispatcher = new ServerDispatcher<GameInstance>(commandDispatcher);
            GameSynchronizer = new ServerGameSynchronizer(commandDispatcher, logger);

            DataMessageHandlerFactory = game => new ServerDataMessageHandler<GameInstance, Player>(game, game.RequestDispatcher, network, logger);
            GameSimulatorFactory = game => new GameController(game);
        }
    }

    sealed class ClientGameContext : IGameContext
    {
        public Logger Logger { get; }
        public IDispatcher<GameInstance> Dispatcher { get; }
        public IRequestDispatcher<GameInstance, Player> RequestDispatcher { get; }
        public IGameSynchronizer GameSynchronizer { get; }
        public Func<GameInstance, IDataMessageHandler<GameInstance, Player>> DataMessageHandlerFactory { get; }
        public Func<GameInstance, IGameController> GameSimulatorFactory { get; }

        public ClientGameContext(ClientNetworkInterface network, Logger logger)
        {
            Logger = logger;

            RequestDispatcher = new ClientRequestDispatcher<GameInstance, Player>(network, logger);
            Dispatcher = new ClientDispatcher<GameInstance>();
            GameSynchronizer = new ClientGameSynchronizer();
            
            DataMessageHandlerFactory = game => new ClientDataMessageHandler<GameInstance, Player>(game, logger);
            GameSimulatorFactory = game => new DummyGameController();
        }
    }
}
