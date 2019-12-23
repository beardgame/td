using System;
using System.Linq;
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
        public ServerLoadingManager(GameInstance game, NetworkInterface networkInterface)
            : base(game, networkInterface) { }

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
            var builder = new GameStateBuilder(Game, getTilemapGenerator());

            var commands = builder.Generate();

            foreach (var command in commands)
            {
                Dispatcher.RunOnlyOnServer(() => command);
            }
        }

        private ITilemapGenerator getTilemapGenerator()
        {
            switch (Game.GameSettings.LevelGenerationMethod)
            {
                case LevelGenerationMethod.Legacy:
                    return new DefaultTilemapGenerator(Logger, Game.LevelDebugMetadata);
                case LevelGenerationMethod.Perlin:
                    return new PerlinTilemapGenerator(Logger, Game.LevelDebugMetadata);
                case LevelGenerationMethod.Empty:
                    return new EmptyTilemapGenerator();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
