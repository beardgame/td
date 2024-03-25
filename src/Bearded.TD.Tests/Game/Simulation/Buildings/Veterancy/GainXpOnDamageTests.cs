using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Testing.Components;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Bearded.TD.Tests.Game.Simulation.Buildings.Veterancy;

public sealed class GainXpOnDamageTests
{
    private readonly ComponentTestBed componentTestBed;

    public GainXpOnDamageTests()
    {
        componentTestBed = ComponentTestBed.CreateInGame();
        componentTestBed.AddComponent(new GainXpOnDamage());
    }

    [Property]
    public void CausingDamageIncreasesXp(PositiveInt damageCaused)
    {
        var d = damageCaused.Get;

        var events = componentTestBed.CollectEvents<GainXp>();

        causeDamage(componentTestBed, d.HitPoints());

        events.Should().Equal(new GainXp(d.Xp()));
    }

    [Fact]
    public void CausingZeroDamageDoesNotIncreaseXp()
    {
        var events = componentTestBed.CollectEvents<GainXp>();

        causeDamage(componentTestBed, HitPoints.Zero);

        events.Should().BeEmpty();
    }

    private static void causeDamage(ComponentTestBed componentTestBed, HitPoints hitPoints)
    {
        var typedDamage = new TypedDamage(hitPoints, DamageType.DivineIntervention);
        componentTestBed.SendEvent(
            new CausedDamage(
                new FinalDamageResult(typedDamage, hitPoints, typedDamage),
                null!));
    }
}
