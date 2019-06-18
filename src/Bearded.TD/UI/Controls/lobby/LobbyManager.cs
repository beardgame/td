using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls
{
    abstract class LobbyManager
    {
        public GameInstance Game { get; }
        protected NetworkInterface Network { get; }
        protected IDispatcher<GameInstance> Dispatcher => Game.Meta.Dispatcher;

        public abstract bool CanChangeGameSettings { get; }

        protected LobbyManager(GameInstance game, NetworkInterface network)
        {
            Game = game;
            Network = network;
        }

        public void ToggleReadyState()
        {
            var connectionState =
                    Game.Me.ConnectionState == PlayerConnectionState.Ready
                        ? PlayerConnectionState.Waiting
                        : PlayerConnectionState.Ready;

            Game.Request(ChangePlayerState.Request(Game.Me, connectionState));
        }

        public void UpdateGameSettings(GameSettings gameSettings)
        {
            DebugAssert.State.Satisfies(CanChangeGameSettings, "Should only set game settings on server.");

            Dispatcher.RunOnlyOnServer(SetGameSettings.Command, Game, gameSettings);
        }

        public void Close() => Network.Shutdown();

        public virtual void Update(UpdateEventArgs args)
        {
            Network.ConsumeMessages();
            Game.UpdatePlayers(args);
        }

        public abstract LoadingManager GetLoadingManager();
    }
}
