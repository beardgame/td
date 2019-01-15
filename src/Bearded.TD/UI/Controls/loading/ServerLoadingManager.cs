﻿using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking;

namespace Bearded.TD.UI.Controls
{
    class ServerLoadingManager : LoadingManager
    {
        private readonly GameSettings gameSettings;

        public ServerLoadingManager(GameInstance game, NetworkInterface networkInterface, GameSettings gameSettings)
            : base(game, networkInterface)
        {
            this.gameSettings = gameSettings;
        }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);

            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.AwaitingLoadingData))
            {
                generateGame();
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
            var radius = gameSettings.LevelSize;

            var tilemapGenerator = new DefaultTilemapGenerator(Logger);
            var builder = new GameStateBuilder(Game, radius, tilemapGenerator);

            var commands = builder.Generate();

            foreach (var command in commands)
            {
                Dispatcher.RunOnlyOnServer(() => command);
            }
        }
    }
}
