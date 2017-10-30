using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Meta;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.UI.Model.Loading;
using Bearded.Utilities;

namespace Bearded.TD.UI.Model.Lobby
{
    abstract class LobbyManager
    {
        public GameInstance Game { get; }
        protected Logger Logger { get; }
        protected IDispatcher Dispatcher { get; }

        protected LobbyManager(IGameContext gameContext, ContentManager contentManager)
        {
            var ids = new IdManager();
            var player = new Player(ids.GetNext<Player>(), getPlayerName())
            {
                ConnectionState = PlayerConnectionState.Waiting
            };
            Game = new GameInstance(gameContext, contentManager, player, ids);
            Logger = gameContext.Logger;
            Dispatcher = gameContext.Dispatcher;
        }

        protected LobbyManager(Player player, IGameContext gameContext, ContentManager contentManager)
        {
            Game = new GameInstance(gameContext, contentManager, player, null);
            Logger = gameContext.Logger;
            Dispatcher = gameContext.Dispatcher;
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

        private static string getPlayerName()
        {
            return UserSettings.Instance.Misc.Username?.Length > 0 ? UserSettings.Instance.Misc.Username : "the host";
        }
    }
}
