using System.Collections.Immutable;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Model;
using FluentAssertions;
using FsCheck.Xunit;

namespace Bearded.TD.Tests.Game.GameLoop;

public sealed class ChapterGeneratorTests
{
    private static readonly ImmutableArray<Element> allElements = ElementExtensions.Enumerate().ToImmutableArray();

    [Property]
    public void ChapterGenerationIsDeterministicGivenSeed(int seed, bool isFirstChapter)
    {
        var gen = new ChapterGenerator(allElements, seed);
        var requirements =
            new ChapterRequirements(isFirstChapter ? 1 : 2, ImmutableArray.Create<double>(100, 150, 200));
        var prevChapter = isFirstChapter
            ? null
            : gen.GenerateChapter(new ChapterRequirements(1, ImmutableArray<double>.Empty), null);

        var script1 = gen.GenerateChapter(requirements, prevChapter);
        var script2 = gen.GenerateChapter(requirements, prevChapter);

        script1.Should().BeEquivalentTo(script2);
    }
}
