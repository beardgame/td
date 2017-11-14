using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Generation;
using Bearded.TD.Networking;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Model.Loading
{
    class ServerLoadingManager : LoadingManager
    {
        public ServerLoadingManager(
            GameInstance game, IDispatcher<GameInstance> dispatcher, NetworkInterface networkInterface, Logger logger)
            : base(game, dispatcher, networkInterface, logger)
        {
        }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);

            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.AwaitingLoadingData))
            {
                generateGame();
                setupFactions();
                Dispatcher.RunOnlyOnServer(AllLoadingDataSent.Command, Game);
            }

            // Also just instantly finish loading for now.
            if (Game.Me.ConnectionState == PlayerConnectionState.ProcessingLoadingData)
            {
                Game.Request(ChangePlayerState.Request, Game.Me, PlayerConnectionState.FinishedLoading);
            }

            // Check if all players finished loading and start the game if so.
            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.FinishedLoading))
            {
                Dispatcher.RunOnlyOnServer(StartGame.Command, Game);
            }
        }

        private void generateGame()
        {
            var radius = Constants.Game.World.Radius;

            var tilemapGenerator = new DefaultTilemapGenerator(Logger);
            var builder = new GameStateBuilder(Game, radius, tilemapGenerator);

            var commands = builder.Generate();

            foreach (var command in commands)
            {
                Dispatcher.RunOnlyOnServer(() => command);
            }
        }

        private void setupFactions()
        {
            foreach (var (p, i) in Game.Players.Indexed())
            {
                var factionColor = Color.FromHSVA(i * Mathf.TwoPi / 6, 1, 1f);
                var playerFaction = new Faction(Game.Ids.GetNext<Faction>(), Game.State.RootFaction, false, factionColor);
                Dispatcher.RunOnlyOnServer(AddFaction.Command, Game, playerFaction);
                Dispatcher.RunOnlyOnServer(SetPlayerFaction.Command, p, playerFaction);
            }
        }
    }
}
