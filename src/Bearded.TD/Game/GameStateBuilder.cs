using System;
using System.Collections.Generic;
using Bearded.TD.Commands;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Loading;
using Bearded.Utilities;

namespace Bearded.TD.Game;

sealed class GameStateBuilder
{
    private readonly GameInstance game;
    private readonly GameSettings gameSettings;
    private readonly ILevelGenerator levelGenerator;

    public GameStateBuilder(GameInstance game, ILevelGenerator levelGenerator)
    {
        this.game = game;
        gameSettings = game.GameSettings;
        this.levelGenerator = levelGenerator;
    }

    public IEnumerable<ISerializableCommand<GameInstance>> Generate()
    {
        // TODO: this is not great, but right now the way that GameInstance works, the seed is sorta broken
        // at the very least we're not keeping track of it in the gameInstance.GameSettings correctly if it's random
        // at worst we've seen potentially unpredictable behaviour, and who knows really...
        // this whole thing needs reviewing (and probably redoing)
        var seed = gameSettings.Seed == 0 ? StaticRandom.Int() : gameSettings.Seed;

        yield return InitializeTypes.Command();
        yield return CreateGameState.Command(game, gameSettings);

        var gameModeId = gameSettings.GameMode ??
            throw new InvalidOperationException("Cannot start game without game mode");
        var gameMode = game.Blueprints.GameModes[gameModeId];
        yield return ApplyGameRules.Command(game, gameMode, seed);

        var levelGenerationParameters = LevelGenerationParametersFactory.Create(game);
        var levelGenerationCommands = levelGenerator.Generate(levelGenerationParameters, seed);

        foreach (var commandFactory in levelGenerationCommands)
        {
            yield return commandFactory(game);
        }

        yield return TurnCrevicesIntoFluidSinks.Command(game);
        yield return TurnEdgesIntoFluidSinks.Command(game);
    }
}
