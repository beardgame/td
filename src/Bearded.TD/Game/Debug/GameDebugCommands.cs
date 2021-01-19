using System;
using Bearded.TD.Game.Commands.Debug;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Utilities.Console;
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

#if DEBUG
        [Command("game.die")]
        private static void die(Logger logger, CommandParameters _) => run(logger, gameInstance =>
        {
            gameInstance.RequestDispatcher.Dispatch(gameInstance.Me, DebugGameOver.Request(gameInstance.State));
        });

        [Command("game.killall")]
        private static void killAll(Logger logger, CommandParameters _) => run(logger, gameInstance =>
        {
            gameInstance.RequestDispatcher.Dispatch(
                gameInstance.Me, KillAllEnemies.Request(gameInstance, DivineIntervention.DamageSource));
        });

        [Command("game.repairall")]
        private static void repairAll(Logger logger, CommandParameters _) => run(logger, gameInstance =>
        {
            gameInstance.RequestDispatcher.Dispatch(gameInstance.Me, RepairAllBuildings.Request(gameInstance));
        });

        [Command("game.resources")]
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

        [Command("game.techpoints")]
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
#endif

        private static void run(Logger logger, Action<GameInstance> command) =>
            DebugGameManager.Instance.RunCommandOrLog(logger, command);
    }
}
