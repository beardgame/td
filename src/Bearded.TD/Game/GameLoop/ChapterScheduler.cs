using System;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using static Bearded.TD.Constants.Game.WaveGeneration;
using static Bearded.TD.Game.GameLoop.WaveScheduler;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.GameLoop;

sealed class ChapterScheduler
{
    private readonly WaveScheduler waveScheduler;
    private readonly ImmutableArray<Element> elements;
    private readonly Random random;
    private readonly Logger logger;

    private ChapterScript? currentChapter;
    private int wavesSpawnedTotal;
    private int wavesSpawnedThisChapter;

    private ElementalTheme? lastChapterElements;

    private ResourceAmount nextWaveResources = FirstWaveResources;

    public event VoidEventHandler? ChapterEnded;

    public ChapterScheduler(WaveScheduler waveScheduler, ImmutableArray<Element> elements, int seed, Logger logger)
    {
        this.waveScheduler = waveScheduler;
        this.elements = elements.IsEmpty ? ImmutableArray.Create(Element.Dynamics) : elements;
        random = new Random(seed);
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
        currentChapter = createChapterScript(chapterRequirements);
        lastChapterElements = currentChapter.Elements;
        wavesSpawnedThisChapter = 0;
        logger.Debug?.Log(
            $"Starting chapter {currentChapter.ChapterNumber} " +
            $"with primary element {currentChapter.Elements.PrimaryElement} " +
            $"and accent element {currentChapter.Elements.AccentElement}");
        requestWave();
    }

    private ChapterScript createChapterScript(ChapterRequirements requirements)
    {
        return new ChapterScript(requirements.ChapterNumber, requirements.WaveCount, chooseChapterElements());
    }

    private ElementalTheme chooseChapterElements()
    {
        // TODO: this is super hardcoded and ugly; needs to be moved to a more generic system loaded from mod files
        if (lastChapterElements is null)
        {
            // Always start the first wave with dynamics and a random accent element. After that, everything goes.
            return new ElementalTheme(Element.Dynamics, chooseAccentElement(Element.Dynamics));
        }

        const int maxAttempts = 5;
        ElementalTheme candidate = default;
        for (var i = 0; i < maxAttempts; i++)
        {
            var primaryElement = elements.RandomElement(random);
            candidate = new ElementalTheme(primaryElement, chooseAccentElement(primaryElement));
            if (candidate.PrimaryElement != lastChapterElements.PrimaryElement &&
                candidate.AccentElement != lastChapterElements.AccentElement)
            {
                return candidate;
            }
        }

        return candidate!;
    }

    private Element chooseAccentElement(Element primaryElement)
    {
        var otherElements = elements.WhereNot(e => e == primaryElement).ToImmutableArray();
        return otherElements.IsEmpty ? primaryElement : otherElements.RandomElement(random);
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
            new WaveEnemyComposition(waveValue, currentChapter.Elements),
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

    public readonly record struct ChapterRequirements(int ChapterNumber, int WaveCount);

    private sealed record ChapterScript(int ChapterNumber, int WaveCount, ElementalTheme Elements);
}
