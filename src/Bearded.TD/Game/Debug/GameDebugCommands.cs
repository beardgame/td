using System;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Debug;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Console;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.Debug
{
    static class GameDebugCommands
    {
        [Command("game.seed")]
        private static void seed(Logger logger, CommandParameters _) => run(logger, gameInstance =>
        {
            logger.Info?.Log(gameInstance.GameSettings.Seed);
        });
        
        [Command("game.killall")]
        private static void killAll(Logger logger, CommandParameters p) => run(logger, gameInstance =>
        {
            gameInstance.RequestDispatcher.Dispatch(KillAllEnemies.Request(gameInstance));
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

        private static void run(Logger logger, Action<GameInstance> command) =>
            DebugGameManager.Instance.RunCommandOrLog(logger, command);
    }
}
