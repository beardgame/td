using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Commands.Debug;
using Bearded.TD.Game.Commands.Loading;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Utilities.Console;
using Bearded.Utilities;
using Bearded.Utilities.IO;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Bearded.TD.Game.Debug
{
    static class GameDebugCommands
    {
        [Command("game.seed")]
        private static void seed(Logger logger, CommandParameters _) => run(logger, gameInstance =>
        {
            logger.Info?.Log(gameInstance.GameSettings.Seed);
        });

        [CommandParameterCompletion("terrain.generators")]
        private static IEnumerable<string> getTerrainGenerators() => Enum.GetNames<LevelGenerationMethod>();

        [DebugCommand("game.generateterrain", "terrain.generators")]
        private static void generateTerrain(Logger logger, CommandParameters p) => run(logger, gameInstance =>
        {
            if (p.Args.Length == 0)
            {
                logger.Warning?.Log("Usage: \"game.generateterrain <method> [seed|'random']\"");
                return;
            }

            if (!Enum.TryParse<LevelGenerationMethod>(p.Args[0], true, out var method))
            {
                logger.Warning?.Log("Valid methods are:");
                foreach (var name in Enum.GetNames<LevelGenerationMethod>())
                {
                    logger.Warning?.Log(name);
                }
                return;
            }

            var seed = p.Args.Length == 1
                ? gameInstance.GameSettings.Seed
                : int.TryParse(p.Args[1], out var s) ? s : StaticRandom.Int();

            logger.Debug?.Log($"Generating new tilemap with method {method} and seed {seed}.");

            gameInstance.LevelDebugMetadata.Clear();

            var generator = TilemapGenerator.From(method, logger, gameInstance.LevelDebugMetadata);

            var tilemap  = generator.Generate(gameInstance.GameSettings.LevelSize, seed);
            var drawInfos = GameStateBuilder.DrawInfosFromTypes(tilemap);

            gameInstance.Meta.Dispatcher.RunOnlyOnServer(FillTilemap.Command, gameInstance, tilemap, drawInfos);

            // TODO: do other work done in GameStateBuilder to create valid playable level?
        });

        [DebugCommand("game.die")]
        private static void die(Logger logger, CommandParameters _) => run(logger, gameInstance =>
        {
            gameInstance.RequestDispatcher.Dispatch(gameInstance.Me, DebugGameOver.Request(gameInstance.State));
        });

        [DebugCommand("game.killall")]
        private static void killAll(Logger logger, CommandParameters _) => run(logger, gameInstance =>
        {
            gameInstance.RequestDispatcher.Dispatch(
                gameInstance.Me, KillAllEnemies.Request(gameInstance, DivineIntervention.DamageSource));
        });

        [DebugCommand("game.repairall")]
        private static void repairAll(Logger logger, CommandParameters _) => run(logger, gameInstance =>
        {
            gameInstance.RequestDispatcher.Dispatch(gameInstance.Me, RepairAllBuildings.Request(gameInstance));
        });

        [DebugCommand("game.resources")]
        private static void giveResources(Logger logger, CommandParameters p) => run(logger, gameInstance =>
        {
            if (p.Args.Length != 1)
            {
                logger.Warning?.Log("Usage: \"game.resources <amount>\"");
                return;
            }

            if (!int.TryParse(p.Args[0], out var amount))
            {
                logger.Warning?.Log($"Invalid amount: {amount}");
                return;
            }

            var faction = gameInstance.Me.Faction;
            while (faction != null && !faction.HasResources)
            {
                faction = faction.Parent;
            }

            if (faction == null)
            {
                logger.Warning?.Log($"Cannot add resources: player is not part of a faction with resource management.");
            }

            gameInstance.RequestDispatcher.Dispatch(gameInstance.Me, GrantResources.Request(faction, amount.Resources()));
        });

        [DebugCommand("game.techpoints")]
        private static void giveTechPoints(Logger logger, CommandParameters p) => run(logger, gameInstance =>
        {
            if (p.Args.Length != 1)
            {
                logger.Warning?.Log("Usage: \"game.techpoints <amount>\"");
                return;
            }

            if (!long.TryParse(p.Args[0], out var number))
            {
                logger.Warning?.Log($"Invalid number: {number}");
                return;
            }

            var faction = gameInstance.Me.Faction;
            while (faction != null && !faction.HasResources)
            {
                faction = faction.Parent;
            }

            if (faction == null)
            {
                logger.Warning?.Log(
                    "Cannot add tech points: player is not part of a faction with technology management.");
            }

            gameInstance.RequestDispatcher.Dispatch(gameInstance.Me, GrantTechPoints.Request(faction, number));
        });

        private static void run(Logger logger, Action<GameInstance> command) =>
            DebugGameManager.Instance.RunCommandOrLog(logger, command);
    }
}
