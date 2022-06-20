using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Testing.Components;
using Bearded.TD.Testing.Factions;
using Bearded.TD.Testing.GameStates;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Simulation.Buildings.Ruins;

public sealed class AutoRepairTests
{
    private readonly GameTestBed gameTestBed;
    private readonly ComponentTestBed componentTestBed;
    private readonly HealthEventReceiver healthEventReceiver;

    public AutoRepairTests()
    {
        gameTestBed = GameTestBed.Create();
        componentTestBed = new ComponentTestBed(gameTestBed);
        healthEventReceiver = new HealthEventReceiver();
        componentTestBed.AddComponent(healthEventReceiver);
        componentTestBed.AddComponent(new FactionProvider(FactionTestFactory.CreateFaction()));
    }

    [Fact]
    public void StartsRepairAfterTimeout()
    {
        componentTestBed.AddComponent(createAutoRepairComponent(1.S(), 1.S()));

        ruinObject(componentTestBed);
        gameTestBed.AdvanceFramesFor(1.S());
        gameTestBed.AdvanceSingleFrame(); // to avoid float rounding errors

        componentTestBed.GetComponents<IIncompleteRepair>().Should().NotBeEmpty();
    }

    [Fact]
    public void FinishesRepairAfterRepairDuration()
    {
        componentTestBed.AddComponent(createAutoRepairComponent(1.S(), 1.S()));

        ruinObject(componentTestBed);
        gameTestBed.AdvanceFramesFor(2.S());
        gameTestBed.AdvanceSingleFrame(); // to avoid float rounding errors

        componentTestBed.GetComponents<IRuined>().Should().BeEmpty();
    }

    [Fact]
    public void ResetsRepairTimerOnDamage()
    {
        componentTestBed.AddComponent(createAutoRepairComponent(1.S(), 1.S()));

        ruinObject(componentTestBed);
        gameTestBed.AdvanceFramesFor(0.6.S());
        doDamage(healthEventReceiver);
        gameTestBed.AdvanceFramesFor(0.6.S());

        componentTestBed.GetComponents<IIncompleteRepair>().Should().BeEmpty();
    }

    [Fact]
    public void InterruptsRepairOnDamage()
    {
        componentTestBed.AddComponent(createAutoRepairComponent(1.S(), 1.S()));

        ruinObject(componentTestBed);
        gameTestBed.AdvanceFramesFor(1.5.S());
        doDamage(healthEventReceiver);
        gameTestBed.AdvanceFramesFor(0.7.S());

        componentTestBed.GetComponents<IRuined>().Should().NotBeEmpty();
        componentTestBed.GetComponents<IIncompleteRepair>().Should().BeEmpty();
    }

    [Fact]
    public void RestartsRepairAfterTimeoutAfterDamage()
    {
        componentTestBed.AddComponent(createAutoRepairComponent(1.S(), 1.S()));

        ruinObject(componentTestBed);
        gameTestBed.AdvanceFramesFor(0.6.S());
        doDamage(healthEventReceiver);
        gameTestBed.AdvanceFramesFor(1.0.S());
        gameTestBed.AdvanceSingleFrame(); // to avoid float rounding errors

        componentTestBed.GetComponents<IIncompleteRepair>().Should().NotBeEmpty();
    }

    [Fact]
    public void RestartsRepairAfterTimeoutAfterInterruption()
    {
        componentTestBed.AddComponent(createAutoRepairComponent(1.S(), 1.S()));

        ruinObject(componentTestBed);
        gameTestBed.AdvanceFramesFor(1.5.S());
        doDamage(healthEventReceiver);
        gameTestBed.AdvanceFramesFor(1.0.S());
        gameTestBed.AdvanceSingleFrame(); // to avoid float rounding errors

        componentTestBed.GetComponents<IIncompleteRepair>().Should().NotBeEmpty();
    }

    private static IComponent createAutoRepairComponent(TimeSpan timeUntilRepairStart, TimeSpan repairDuration)
    {
        return new AutoRepair(
            new AutoRepairParametersTemplate(timeUntilRepairStart, repairDuration, resetTimerOnDamage: true));
    }

    private static void ruinObject(ComponentTestBed componentTestBed)
    {
        componentTestBed.AddComponent(new Ruined(new RuinedParametersTemplate(null)));
    }

    private static void doDamage(IHealthEventReceiver healthEventReceiver)
    {
        healthEventReceiver.Damage(new TypedDamage(1.HitPoints(), DamageType.Kinetic), DivineIntervention.DamageSource);
    }
}
