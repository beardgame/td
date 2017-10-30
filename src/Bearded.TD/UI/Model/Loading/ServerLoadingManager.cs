using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Generation;
using Bearded.TD.Mods.Models;
using Bearded.TD.Networking;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.Math;

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

            if (Game.Me.ConnectionState == PlayerConnectionState.DownloadingMods)
            {
                if (!HasModsQueuedForLoading)
                    Game.ContentManager.Mods.ForEach(LoadMod);
                else if (HasModsQueuedForLoading && HaveAllModsFinishedLoading)
                    Game.RequestDispatcher.Dispatch(
                        ChangePlayerState.Request(Game.Me, PlayerConnectionState.AwaitingLoadingData));
            }

            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.AwaitingLoadingData))
            {
                generateGame();
                setupFactions();
                Dispatcher.RunOnlyOnServer(AllLoadingDataSent.Command, Game);
            }

            // Also just instantly finish loading for now.
            if (Game.Me.ConnectionState == PlayerConnectionState.ProcessingLoadingData)
                Game.Request(ChangePlayerState.Request, Game.Me, PlayerConnectionState.FinishedLoading);

            // Check if all players finished loading and start the game if so.
            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.FinishedLoading))
                Dispatcher.RunOnlyOnServer(StartGame.Command, Game);
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
            
            foreach (var mod in Game.Mods)
            {
                mod.Units.All.ForEach(
                    blueprint => Dispatcher.RunOnlyOnServer(() => SendUnitBlueprint.Command(Game, blueprint)));
            }
        }
    }
}
