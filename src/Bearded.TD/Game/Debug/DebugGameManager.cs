using System;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Game.Debug
{
    sealed class DebugGameManager
    {
        public static DebugGameManager Instance = new DebugGameManager();

        private DebugGameManager() { }

        public Maybe<GameInstance> Game { get; private set; }

        public void RegisterGame(GameInstance game)
        {
            Game.Match(
                onValue: _ => throw new InvalidOperationException("Cannot register a game if there is already one."),
                onNothing: () => Game = Just(game)
            );
        }

        public void UnregisterGame()
        {
            Game.Match(
                onValue: _ => Game = Nothing,
                onNothing: () => throw new InvalidOperationException("Cannot unregister a game if there is none.")
            );
        }

        public void RunCommandOrLog(Logger logger, Action<GameInstance> command)
        {
            if (!TryRunCommand(command))
            {
                logger.Warning?.Log("Cannot run game command, because there is no active game,");
            }
        }

        public bool TryRunCommand(Action<GameInstance> command)
        {
            var commandRan = false;
            Game.Match(
                onValue: game =>
                {
                    command(game);
                    commandRan = true;
                },
                onNothing: () => commandRan = false
            );
            return commandRan;
        }
    }
}
