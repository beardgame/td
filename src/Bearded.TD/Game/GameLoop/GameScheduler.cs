using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.GameLoop;
using static Bearded.TD.Utilities.DebugAssert;
using static Bearded.TD.Constants.Game.WaveGeneration;

namespace Bearded.TD.Game.GameLoop;

sealed class GameScheduler(
    GameState game,
    ICommandDispatcher<GameInstance> commandDispatcher,
    ChapterGenerator chapterGenerator,
    ChapterExecutor chapterExecutor,
    GameScheduler.GameRequirements gameRequirements)
{
    private bool gameStarted;
    private int chaptersStarted;
    private ChapterScript? previousChapter;

    private void onChapterEnded()
    {
        if (chaptersStarted < gameRequirements.ChaptersPerGame)
        {
            requestChapter();
        }
        else
        {
            endGame();
        }
    }

    public void StartGame()
    {
        State.Satisfies(!gameStarted);
        gameStarted = true;

        commandDispatcher.Dispatch(
            SetGameDuration.Command(game, gameRequirements.ChaptersPerGame, gameRequirements.WavesPerChapter));
        requestChapter();
    }

    private void endGame()
    {
        State.Satisfies(gameStarted);
        commandDispatcher.Dispatch(WinGame.Command(game));
    }

    private void requestChapter()
    {
        State.Satisfies(chaptersStarted < gameRequirements.ChaptersPerGame);
        var chapterNumber = ++chaptersStarted;
        var requirements = new ChapterRequirements(chapterNumber, waveThreats(chapterNumber));
        var script = chapterGenerator.GenerateChapter(requirements, previousChapter);
        previousChapter = script;
        chapterExecutor.ExecuteScript(script, onChapterEnded);
    }

    private ImmutableArray<double> waveThreats(int chapterNumber)
    {
        var totalWavesSpawned = (chapterNumber - 1) * gameRequirements.WavesPerChapter;
        return Enumerable.Range(totalWavesSpawned, gameRequirements.WavesPerChapter)
            .Select(i =>
                FirstWaveValue +
                WaveValueLinearGrowth * i * Math.Pow(WaveValueExponentialGrowth, i))
            .ToImmutableArray();
    }

    public sealed record GameRequirements(int ChaptersPerGame, int WavesPerChapter);
}
