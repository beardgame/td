using System;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities.Console;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.Debug
{
    static class GameDebugCommands
    {
        [Command("game.killall")]
        private static void killAll(Logger logger, CommandParameters p) => run(logger, gameInstance =>
        {
            foreach (var enemy in gameInstance.State.GameObjects.OfType<EnemyUnit>())
            {
                enemy.Sync(KillUnit.Command, enemy, gameInstance.State.RootFaction);
            }
        });

        private static void run(Logger logger, Action<GameInstance> command) =>
            DebugGameManager.Instance.RunCommandOrLog(logger, command);
    }
}
