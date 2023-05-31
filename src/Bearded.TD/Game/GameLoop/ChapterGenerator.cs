using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Linq;
using static Bearded.TD.Constants.Game.WaveGeneration;

namespace Bearded.TD.Game.GameLoop;

sealed class ChapterGenerator
{
    private readonly ImmutableArray<Element> elements;
    private readonly int seed;

    public ChapterGenerator(ImmutableArray<Element> elements, int seed)
    {
        this.elements = elements.IsEmpty ? ImmutableArray.Create(Element.Force) : elements;
        this.seed = seed;
    }

    public ChapterScript GenerateChapter(ChapterRequirements requirements, ChapterScript? previousChapter)
    {
        // Ensure that a change in requirements always leads to a (very) different outcome.
        var random = new Random(seed ^ requirements.GetHashCode());

        return new ChapterScript(
            requirements.ChapterNumber,
            generateWaveDescriptions(requirements.Waves),
            chooseChapterElements(previousChapter, random));
    }

    private ImmutableArray<WaveDescription> generateWaveDescriptions(ImmutableArray<double> waveThreats)
    {
        return waveThreats
            .Select((threat, i) => new WaveDescription(threat, i == 0 ? FirstDownTimeDuration : DownTimeDuration))
            .ToImmutableArray();
    }

    private ElementalTheme chooseChapterElements(ChapterScript? previousChapter, Random random)
    {
        // TODO: this is super hardcoded and ugly; needs to be moved to a more generic system loaded from mod files
        if (previousChapter is not { } prevChapter)
        {
            // Always start the first wave with force and a random accent element. After that, everything goes.
            const Element firstChapterElement = Element.Force;
            return new ElementalTheme(firstChapterElement, chooseAccentElement(firstChapterElement, random));
        }

        const int maxAttempts = 5;
        ElementalTheme candidate = default;
        for (var i = 0; i < maxAttempts; i++)
        {
            var primaryElement = elements.RandomElement(random);
            candidate = new ElementalTheme(primaryElement, chooseAccentElement(primaryElement, random));
            if (candidate.PrimaryElement != prevChapter.Elements.PrimaryElement &&
                candidate.AccentElement != prevChapter.Elements.AccentElement)
            {
                return candidate;
            }
        }

        return candidate!;
    }

    private Element chooseAccentElement(Element primaryElement, Random random)
    {
        var otherElements = elements.WhereNot(e => e == primaryElement).ToImmutableArray();
        return otherElements.IsEmpty ? primaryElement : otherElements.RandomElement(random);
    }
}
