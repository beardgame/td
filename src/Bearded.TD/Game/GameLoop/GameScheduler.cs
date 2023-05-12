using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.GameLoop;
using static Bearded.TD.Utilities.DebugAssert;
using static Bearded.TD.Constants.Game.WaveGeneration;

namespace Bearded.TD.Game.GameLoop;

sealed class GameScheduler
{
    private readonly GameState game;
    private readonly ICommandDispatcher<GameInstance> commandDispatcher;
    private readonly ChapterGenerator chapterGenerator;
    private readonly ChapterDirector chapterDirector;
    private readonly GameRequirements gameRequirements;

    private bool gameStarted;
    private int chaptersStarted;
    private ChapterScript? previousChapter;

    public GameScheduler(
        GameState game,
        ICommandDispatcher<GameInstance> commandDispatcher,
        ChapterGenerator chapterGenerator,
        ChapterDirector chapterDirector,
        GameRequirements gameRequirements)
    {
        this.game = game;
        this.commandDispatcher = commandDispatcher;
        this.chapterGenerator = chapterGenerator;
        this.chapterDirector = chapterDirector;
        this.gameRequirements = gameRequirements;
    }

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

        requestChapter();
    }

    private void endGame()
    {
        State.Satisfies(gameStarted);
        commandDispatcher.Dispatch(WinGame.Command(game));
    }

    private void requestChapter()
    {
        State.Satisfies(chaptersStarted < gameRequirements.WavesPerChapter);
        var chapterNumber = ++chaptersStarted;
        var requirements = new ChapterRequirements(chapterNumber, waveThreats(chapterNumber));
        var script = chapterGenerator.GenerateChapter(requirements, previousChapter);
        previousChapter = script;
        chapterDirector.ExecuteScript(script, onChapterEnded);
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
