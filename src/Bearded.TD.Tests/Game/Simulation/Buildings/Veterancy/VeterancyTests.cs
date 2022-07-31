using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Testing.Components;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Simulation.Buildings.Veterancy;

using Veterancy = Bearded.TD.Game.Simulation.Buildings.Veterancy.Veterancy;

public sealed class VeterancyTests
{
    private readonly ComponentTestBed componentTestBed;

    public VeterancyTests()
    {
        componentTestBed = ComponentTestBed.Activated();
    }

    [Theory]
    [InlineData(50)]
    [InlineData(99)]
    public void GainingXpBelowThresholdDoesNotGiveLevel(int xp)
    {
        componentTestBed.AddComponent(Veterancy.WithLevelThresholds(100.Xp()));
        var events = componentTestBed.CollectEvents<GainLevel>();

        gainXp(componentTestBed, xp.Xp());

        events.Should().BeEmpty();
    }

    [Theory]
    [InlineData(100)]
    [InlineData(199)]
    public void GainingXpOverThresholdDoesGiveLevel(int xp)
    {
        componentTestBed.AddComponent(Veterancy.WithLevelThresholds(100.Xp(), 200.Xp()));
        var events = componentTestBed.CollectEvents<GainLevel>();

        gainXp(componentTestBed, xp.Xp());

        events.Should().Equal(new GainLevel());
    }

    [Fact]
    public void GainingXpOverMultipleThresholdsGivesMultipleLevels()
    {
        componentTestBed.AddComponent(Veterancy.WithLevelThresholds(100.Xp(), 200.Xp(), 300.Xp()));
        var events = componentTestBed.CollectEvents<GainLevel>();

        gainXp(componentTestBed, 250.Xp());

        events.Should().Equal(new GainLevel(), new GainLevel());
    }

    [Theory]
    [InlineData(3, 34)]
    [InlineData(4, 25)]
    public void GainingXpInMultipleStepsGivesLevel(int times, int xp)
    {
        componentTestBed.AddComponent(Veterancy.WithLevelThresholds(100.Xp(), 200.Xp()));
        var events = componentTestBed.CollectEvents<GainLevel>();

        for (var i = 0; i < times; i++)
        {
            gainXp(componentTestBed, xp.Xp());
        }

        events.Should().Equal(new GainLevel());
    }

    private static void gainXp(ComponentTestBed componentTestBed, Experience xp)
    {
        componentTestBed.SendEvent(new GainXp(xp));
    }
}
