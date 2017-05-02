using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Players;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;

namespace Bearded.TD.Networking.Lobby
{
    abstract class LobbyManager
    {
        public Logger Logger { get; }
        public GameInstance Game { get; }
        protected IDispatcher Dispatcher { get; }

        protected LobbyManager(Logger logger,
            (IRequestDispatcher request, IDispatcher master) dispatchers)
            : this(logger, dispatchers.master)
        {
            var ids = new IdManager();
            var player = new Player(ids.GetNext<Player>(), "The host", Color.Red);
            Game = new GameInstance(player, dispatchers.request, ids);
        }

        protected LobbyManager(Logger logger, Player player,
            (IRequestDispatcher request, IDispatcher master) dispatchers)
            : this(logger, dispatchers.master)
        {
            Game = new GameInstance(player, dispatchers.request, null);
        }

        private LobbyManager(Logger logger, IDispatcher dispatcher)
        {
            Logger = logger;
            Dispatcher = dispatcher;
        }

        public abstract void Update(UpdateEventArgs args);

        public void ToggleReadyState()
        {
            var connectionState =
                    Game.Me.ConnectionState == PlayerConnectionState.Ready
                        ? PlayerConnectionState.Waiting
                        : PlayerConnectionState.Ready;

            Game.RequestDispatcher.Dispatch(ChangePlayerState.Request(Game.Me, connectionState));
        }

        public GameInstance GetStartedInstance(InputManager inputManager)
        {
            var meta = new GameMeta(Logger, Dispatcher, Game.Ids);
            var gameState = GameStateBuilder.Generate(meta, new DefaultTilemapGenerator(Logger));
            var camera = new GameCamera(inputManager, meta, gameState.Level.Tilemap.Radius);

            Game.Start(gameState, camera);

            return Game;
        }
    }
}