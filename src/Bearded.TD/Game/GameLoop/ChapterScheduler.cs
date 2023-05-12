using System;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Model;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using static Bearded.TD.Constants.Game.WaveGeneration;
using static Bearded.TD.Game.GameLoop.WaveScheduler;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.GameLoop;

sealed class ChapterScheduler
{
    private readonly ChapterGenerator generator;
    private readonly WaveScheduler waveScheduler;
    private readonly Logger logger;

    private ChapterScript? currentChapter;
    private ChapterScript? lastChapter;
    private int wavesSpawnedTotal;
    private int wavesSpawnedThisChapter;

    public event VoidEventHandler? ChapterEnded;

    public ChapterScheduler(WaveScheduler waveScheduler, ImmutableArray<Element> elements, int seed, Logger logger)
    {
        generator = new ChapterGenerator(elements, seed);
        this.waveScheduler = waveScheduler;
        this.logger = logger;
        waveScheduler.WaveEnded += onWaveEnded;
    }

    public void OnGameStart()
    {
        waveScheduler.OnGameStart();
    }

    private void onWaveEnded()
    {
        State.Satisfies(currentChapter != null);
        if (wavesSpawnedThisChapter < currentChapter!.Value.WaveCount)
        {
            requestWave();
        }
        else
        {
            endChapter();
        }
    }

    public void StartChapter(ChapterRequirements chapterRequirements)
    {
        State.Satisfies(currentChapter == null);
        currentChapter = generator.GenerateChapter(chapterRequirements, lastChapter);
        var chapter = currentChapter.Value;
        wavesSpawnedThisChapter = 0;
        logger.Debug?.Log(
            $"Starting chapter {chapter.ChapterNumber} " +
            $"with primary element {chapter.Elements.PrimaryElement} " +
            $"and accent element {chapter.Elements.AccentElement}");
        requestWave();
    }

    private void requestWave()
    {
        State.Satisfies(currentChapter != null);
        State.Satisfies(wavesSpawnedThisChapter < currentChapter!.Value.WaveCount);
        var waveNumber = ++wavesSpawnedThisChapter;
        var waveValue = FirstWaveValue +
            WaveValueLinearGrowth * wavesSpawnedTotal * Math.Pow(WaveValueExponentialGrowth, wavesSpawnedTotal);
        wavesSpawnedTotal++;
        waveScheduler.StartWave(new WaveRequirements(
            currentChapter.Value.ChapterNumber,
            waveNumber,
            new WaveEnemyComposition(waveValue, currentChapter.Value.Elements),
            waveNumber == 1 ? FirstDownTimeDuration : DownTimeDuration));
    }

    private void endChapter()
    {
        State.Satisfies(currentChapter != null);
        lastChapter = currentChapter;
        currentChapter = null;
        ChapterEnded?.Invoke();
    }
}
