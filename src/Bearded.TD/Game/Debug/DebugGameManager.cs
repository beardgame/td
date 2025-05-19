using System;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.Debug;

sealed class DebugGameManager
{
    public static DebugGameManager Instance { get; } = new();

    private DebugGameManager() { }

    public GameInstance? Game { get; private set; }

    public void RegisterGame(GameInstance game)
    {
        if (Game is not null)
        {
            throw new InvalidOperationException("Cannot register a game if there is already one.");
        }

        Game = game;
    }

    public void UnregisterGame()
    {
        if (Game is null)
        {
            throw new InvalidOperationException("Cannot unregister a game if there is nothing.");
        }

        Game = null;
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
        if (Game is { } game)
        {
            command(game);
        }
        return Game is not null;
    }
}
