using System;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Buildings.Components;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Networking.Loading
{
    class ServerLoadingManager : LoadingManager
    {
        public ServerLoadingManager(
            GameInstance game, IDispatcher dispatcher, NetworkInterface networkInterface, Logger logger)
            : base(game, dispatcher, networkInterface, logger)
        {
        }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);

            // Just instantly finish sending everything.
            if (Game.Me.ConnectionState == PlayerConnectionState.AwaitingLoadingData)
                generateGame();

            // Also just instantly finish loading for now.
            if (Game.Me.ConnectionState == PlayerConnectionState.ProcessingLoadingData)
                Game.Request(ChangePlayerState.Request, Game.Me, PlayerConnectionState.FinishedLoading);

            // Check if all players finished loading and start the game if so.
            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.FinishedLoading))
                Dispatcher.RunOnlyOnServer(StartGame.Command, Game);
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

            Dispatcher.RunOnlyOnServer(AllLoadingDataSent.Command, Game);
        }

        private void debug_sendBlueprints()
        {
            // In the future these would be loaded from a mod file.
            Game.Blueprints.Buildings.RegisterBlueprint("wall",
                new BuildingBlueprint(Game.Ids.GetNext<BuildingBlueprint>(),
                    TileSelection.FromFootprints(FootprintGroup.Single), 100, 5, null));
            Game.Blueprints.Buildings.RegisterBlueprint("triangle",
                new BuildingBlueprint(Game.Ids.GetNext<BuildingBlueprint>(),
                    TileSelection.FromFootprints(FootprintGroup.Triangle), 300, 20,
                    new Func<Component>[] {() => new Turret()}));
        }
    }
}
