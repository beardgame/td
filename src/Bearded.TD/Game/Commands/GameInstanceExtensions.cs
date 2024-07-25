using System;

namespace Bearded.TD.Game.Commands;

static class GameInstanceExtensions
{
    public static void MustBeLoading(this GameInstance game)
    {
        if (game.Status != GameStatus.Loading)
            throw new InvalidOperationException("Game must be loading.");
    }
}