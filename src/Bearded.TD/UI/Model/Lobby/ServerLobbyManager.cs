using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.UI.Model.Loading;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Model.Lobby
{
    class ServerLobbyManager : LobbyManager
    {
        private const float heartbeatTimeSeconds = 10;

        private readonly ServerNetworkInterface networkInterface;
        private float secondsUntilNextHeartbeat;

        public ServerLobbyManager(ServerNetworkInterface networkInterface, Logger logger, ContentManager contentManager)
            : base(new ServerGameContext(networkInterface, logger), contentManager)
        {
            this.networkInterface = networkInterface;

            // First heartbeat automatically registers lobby.
            doHeartbeat();
        }

        public override void Update(UpdateEventArgs args)
        {
            networkInterface.ConsumeMessages();
            Game.UpdatePlayers(args);

            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.Ready))
            {
                Dispatcher.RunOnlyOnServer(
                    commandDispatcher => commandDispatcher.Dispatch(BeginLoadingGame.Command(Game)));
            }

            secondsUntilNextHeartbeat -= args.ElapsedTimeInSf;
            if (secondsUntilNextHeartbeat <= 0)
            {
                doHeartbeat();
            }
        }

        private void doHeartbeat()
        {
            networkInterface.Master.RegisterLobby(
                new Proto.Lobby
                {
                    Id = networkInterface.UniqueIdentifier,
                    Name = $"{Game.Me.Name}'s game",
                    MaxNumPlayers = 4,
                    CurrentNumPlayers = 1,
                }
            );
            secondsUntilNextHeartbeat = heartbeatTimeSeconds;
        }

        

        public override LoadingManager GetLoadingManager()
        {
            return new ServerLoadingManager(Game, Dispatcher, networkInterface, Logger);
        }
    }
}