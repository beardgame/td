using System;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.Utilities;
using static Bearded.TD.Constants.Game.WaveGeneration;
using static Bearded.TD.Game.GameLoop.WaveScheduler;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.GameLoop;

sealed class ChapterScheduler
{
    private readonly WaveScheduler waveScheduler;

    private ChapterScript? currentChapter;
    private int wavesSpawnedTotal;
    private int wavesSpawnedThisChapter;

    private ResourceAmount nextWaveResources = FirstWaveResources;

    public event VoidEventHandler? ChapterEnded;

    public ChapterScheduler(WaveScheduler waveScheduler)
    {
        this.waveScheduler = waveScheduler;
        waveScheduler.WaveEnded += onWaveEnded;
    }

    public void OnGameStart()
    {
        waveScheduler.OnGameStart();
    }

    private void onWaveEnded()
    {
        State.Satisfies(currentChapter != null);
        if (wavesSpawnedThisChapter < currentChapter!.WaveCount)
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
        currentChapter = new ChapterScript(chapterRequirements.ChapterNumber, chapterRequirements.WaveCount);
        wavesSpawnedThisChapter = 0;
        requestWave();
    }

    private void requestWave()
    {
        State.Satisfies(currentChapter != null);
        State.Satisfies(wavesSpawnedThisChapter < currentChapter!.WaveCount);
        var waveNumber = ++wavesSpawnedThisChapter;
        var waveValue = FirstWaveValue +
            WaveValueLinearGrowth * wavesSpawnedTotal * Math.Pow(WaveValueExponentialGrowth, wavesSpawnedTotal);
        wavesSpawnedTotal++;
        waveScheduler.StartWave(new WaveRequirements(
            currentChapter.ChapterNumber,
            waveNumber,
            waveValue,
            nextWaveResources,
            waveNumber == 1 ? FirstDownTimeDuration : DownTimeDuration));

        nextWaveResources = new ResourceAmount((int) (nextWaveResources.NumericValue * WaveResourcesMultiplier));
    }

    private void endChapter()
    {
        State.Satisfies(currentChapter != null);
        currentChapter = null;
        ChapterEnded?.Invoke();
    }

    public readonly struct ChapterRequirements
    {
        public int ChapterNumber { get; }
        public int WaveCount { get; }

        public ChapterRequirements(int chapterNumber, int waveCount)
        {
            WaveCount = waveCount;
            ChapterNumber = chapterNumber;
        }
    }

    private sealed record ChapterScript(int ChapterNumber, int WaveCount);
}
