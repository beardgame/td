using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    interface IGameContext
    {
        Logger Logger { get; }
        IDispatcher Dispatcher { get; }
        IRequestDispatcher RequestDispatcher { get; }
        IGameSynchronizer GameSynchronizer { get; }
        Func<GameInstance, IDataMessageHandler> DataMessageHandlerFactory { get; }
    }

    sealed class ServerGameContext : IGameContext
    {
        public Logger Logger { get; }
        public IDispatcher Dispatcher { get; }
        public IRequestDispatcher RequestDispatcher { get; }
        public IGameSynchronizer GameSynchronizer { get; }
        public Func<GameInstance, IDataMessageHandler> DataMessageHandlerFactory { get; }

        public ServerGameContext(ServerNetworkInterface network, Logger logger)
        {
            Logger = logger;

            var commandDispatcher = new ServerCommandDispatcher(new DefaultCommandExecutor(), network);
            RequestDispatcher = new ServerRequestDispatcher(commandDispatcher, logger);
            Dispatcher = new ServerDispatcher(commandDispatcher);
            GameSynchronizer = new ServerGameSynchronizer(commandDispatcher, logger);

            DataMessageHandlerFactory = game => new ServerDataMessageHandler(game, network, logger);
        }
    }

    sealed class ClientGameContext : IGameContext
    {
        public Logger Logger { get; }
        public IDispatcher Dispatcher { get; }
        public IRequestDispatcher RequestDispatcher { get; }
        public IGameSynchronizer GameSynchronizer { get; }
        public Func<GameInstance, IDataMessageHandler> DataMessageHandlerFactory { get; }

        public ClientGameContext(ClientNetworkInterface network, Logger logger)
        {
            Logger = logger;

            RequestDispatcher = new ClientRequestDispatcher(network, logger);
            Dispatcher = new ClientDispatcher();
            GameSynchronizer = new ClientGameSynchronizer();
            
            DataMessageHandlerFactory = game => new ClientDataMessageHandler(game, logger);
        }
    }
}
