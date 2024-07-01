using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game;

interface IGameContext
{
    Logger Logger { get; }
    IDispatcher<GameInstance> Dispatcher { get; }
    IRequestDispatcher<Player, GameInstance> RequestDispatcher { get; }
    IGameSynchronizer GameSynchronizer { get; }
    Func<GameInstance, PlayerManager?> PlayerManagerFactory { get; }
    Action<GameInstance> DataMessageHandlerInitializer { get; }
}

sealed class ServerGameContext : IGameContext
{
    public Logger Logger { get; }
    public IDispatcher<GameInstance> Dispatcher { get; }
    public IRequestDispatcher<Player, GameInstance> RequestDispatcher { get; }
    public IGameSynchronizer GameSynchronizer { get; }
    public Func<GameInstance, PlayerManager?> PlayerManagerFactory { get; }
    public Action<GameInstance> DataMessageHandlerInitializer { get; }

    public ServerGameContext(ServerNetworkInterface network, Logger logger)
    {
        Logger = logger;

        var commandDispatcher = new ServerCommandDispatcher<Player, GameInstance>(new DefaultCommandExecutor(), network);
        RequestDispatcher = new ServerRequestDispatcher<Player, GameInstance>(commandDispatcher);
        Dispatcher = new ServerDispatcher<GameInstance>(commandDispatcher);
        GameSynchronizer = new ServerGameSynchronizer(commandDispatcher);
        PlayerManagerFactory = game =>
        {
            var playerManager = new PlayerManager(game, network, Dispatcher);
            network.RegisterMessageHandler(playerManager);
            return playerManager;
        };
        DataMessageHandlerInitializer = game => network.RegisterMessageHandler(new ServerDataMessageHandler(game, logger));
    }
}

sealed class ClientGameContext : IGameContext
{
    public Logger Logger { get; }
    public IDispatcher<GameInstance> Dispatcher { get; }
    public IRequestDispatcher<Player, GameInstance> RequestDispatcher { get; }
    public IGameSynchronizer GameSynchronizer { get; }
    public Func<GameInstance, PlayerManager?> PlayerManagerFactory { get; }
    public Action<GameInstance> DataMessageHandlerInitializer { get; }

    public ClientGameContext(ClientNetworkInterface network, Logger logger)
    {
        Logger = logger;

        RequestDispatcher = new ClientRequestDispatcher<Player, GameInstance>(network);
        Dispatcher = new ClientDispatcher<GameInstance>();
        GameSynchronizer = new ClientGameSynchronizer();
        PlayerManagerFactory = _ => null;
        DataMessageHandlerInitializer = game => network.RegisterMessageHandler(new ClientDataMessageHandler(game, logger));
    }
}
