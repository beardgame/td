using System;
using Bearded.TD.Game.Commands.Debug;
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
            gameInstance.RequestDispatcher.Dispatch(DebugGameOver.Request(gameInstance.State));
        });

        [Command("game.killall")]
        private static void killAll(Logger logger, CommandParameters _) => run(logger, gameInstance =>
        {
            gameInstance.RequestDispatcher.Dispatch(KillAllEnemies.Request(gameInstance));
        });

        [Command("game.repairall")]
        private static void repairAll(Logger logger, CommandParameters _) => run(logger, gameInstance =>
        {
            gameInstance.RequestDispatcher.Dispatch(RepairAllBuildings.Request(gameInstance));
        });

        [Command("game.resources")]
        private static void giveResources(Logger logger, CommandParameters p) => run(logger, gameInstance =>
        {
            if (p.Args.Length != 1)
            {
                logger.Warning?.Log("Usage: \"game.resources <amount>\"");
                return;
            }

            if (!double.TryParse(p.Args[0], out var amount))
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

            gameInstance.RequestDispatcher.Dispatch(GrantResources.Request(faction, amount));
        });
#endif

        private static void run(Logger logger, Action<GameInstance> command) =>
            DebugGameManager.Instance.RunCommandOrLog(logger, command);
    }
}
