using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Units;
using Bearded.TD.Mods.Models;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Loading;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI.Model.Loading
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
            {
                generateGame();
                setupFactions();
            }

            // Also just instantly finish loading for now.
            if (Game.Me.ConnectionState == PlayerConnectionState.ProcessingLoadingData)
                Game.Request(ChangePlayerState.Request, Game.Me, PlayerConnectionState.FinishedLoading);

            // Check if all players finished loading and start the game if so.
            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.FinishedLoading))
                Dispatcher.RunOnlyOnServer(StartGame.Command, Game);
        }

        private void setupFactions()
        {
            foreach (var (p, i) in Game.Players.Indexed())
            {
                var factionColor = Color.FromHSVA(i * Mathf.TwoPi / 6, 1, 1f);
                var playerFaction = new Faction(Game.Ids.GetNext<Faction>(), Game.State.RootFaction, false, factionColor);
                Dispatcher.RunOnlyOnServer(AddFaction.Command, Game, playerFaction);
                Dispatcher.RunOnlyOnServer(SetPlayerFaction.Command,  p, playerFaction);
            }
        }

        private void generateGame()
        {
            Dispatcher.RunOnlyOnServer(() => LoadBasicData.Command(Game));
            debug_sendBlueprints();

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
            // === Buildings ===
            Dispatcher.RunOnlyOnServer(SendBuildingBlueprint.Command, Game, 
            new BuildingBlueprint(Game.Ids.GetNext<BuildingBlueprint>(), "base", FootprintGroup.CircleSeven, 1000, 1,
                new[] {
                    Game.Blueprints.Components["sink"],
                    Game.Blueprints.Components["income_over_time"],
                    Game.Blueprints.Components["game_over_on_destroy"],
                    Game.Blueprints.Components["worker_hub"],
                }));
            // In the future these would be loaded from a mod file.
            Dispatcher.RunOnlyOnServer(() => SendBuildingBlueprint.Command(Game,
                new BuildingBlueprint(Game.Ids.GetNext<BuildingBlueprint>(), "wall", FootprintGroup.Single, 100, 15,
                    null)));
            Dispatcher.RunOnlyOnServer(() => SendBuildingBlueprint.Command(Game,
                new BuildingBlueprint(Game.Ids.GetNext<BuildingBlueprint>(), "triangle", FootprintGroup.Triangle, 300,
                    75, Game.Blueprints.Components["turret"].Yield())));
            Dispatcher.RunOnlyOnServer(() => SendBuildingBlueprint.Command(Game,
                new BuildingBlueprint(Game.Ids.GetNext<BuildingBlueprint>(), "diamond", FootprintGroup.Diamond, 200, 40,
                    null)));
            Dispatcher.RunOnlyOnServer(() => SendBuildingBlueprint.Command(Game,
                new BuildingBlueprint(Game.Ids.GetNext<BuildingBlueprint>(), "line", FootprintGroup.Line, 150, 25,
                    null)));

            // === Enemies ===
            Dispatcher.RunOnlyOnServer(() => SendUnitBlueprint.Command(Game, new UnitBlueprint(
                Game.Ids.GetNext<UnitBlueprint>(), "debug", 100, 10, 2.S(), new Speed(2), 2, Color.DarkRed)));
            Dispatcher.RunOnlyOnServer(() => SendUnitBlueprint.Command(Game, new UnitBlueprint(
                Game.Ids.GetNext<UnitBlueprint>(), "strong", 250, 20, 1.5.S(), new Speed(1.2f), 4, Color.Yellow)));
            Dispatcher.RunOnlyOnServer(() => SendUnitBlueprint.Command(Game, new UnitBlueprint(
                Game.Ids.GetNext<UnitBlueprint>(), "fast", 50, 4, .5.S(), new Speed(3), 4, Color.CornflowerBlue)));
            Dispatcher.RunOnlyOnServer(() => SendUnitBlueprint.Command(Game, new UnitBlueprint(
                Game.Ids.GetNext<UnitBlueprint>(), "tank", 1000, 50, 2.S(), new Speed(.8f), 12, Color.SandyBrown)));
        }
    }
}
