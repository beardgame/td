using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Testing.Components;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Simulation.Damage;

public sealed class DamageTests
{
    private readonly ComponentTestBed testBed;
    private readonly HealthEventReceiver healthEventReceiver;

    public DamageTests()
    {
        testBed = ComponentTestBed.CreateOrphaned();
        healthEventReceiver = new HealthEventReceiver();
        testBed.AddComponent(healthEventReceiver);
    }

    [Fact]
    public void DamageReducesHitPoints()
    {
        var hp = hitPoints(100.HitPoints(), DamageShell.Health);
        testBed.AddComponent(hp);

        doDamage(10.HitPoints());

        hp.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void DamageReducesShieldBeforeArmor()
    {
        var a = hitPoints(100.HitPoints(), DamageShell.Armor);
        testBed.AddComponent(a);
        var s = hitPoints(100.HitPoints(), DamageShell.Shield);
        testBed.AddComponent(s);

        doDamage(10.HitPoints());

        a.CurrentHitPoints.Should().Be(100.HitPoints());
        s.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void DamageReducesArmorBeforeHealth()
    {
        var a = hitPoints(100.HitPoints(), DamageShell.Armor);
        testBed.AddComponent(a);
        var h = hitPoints(100.HitPoints(), DamageShell.Health);
        testBed.AddComponent(h);

        doDamage(10.HitPoints());

        a.CurrentHitPoints.Should().Be(90.HitPoints());
        h.CurrentHitPoints.Should().Be(100.HitPoints());
    }

    [Fact]
    public void DamageToShellDoesNotOverflow()
    {
        var s = hitPoints(100.HitPoints(), DamageShell.Shield);
        testBed.AddComponent(s);
        var h = hitPoints(100.HitPoints(), DamageShell.Health);
        testBed.AddComponent(h);

        doDamage(150.HitPoints());

        s.CurrentHitPoints.Should().Be(0.HitPoints());
        h.CurrentHitPoints.Should().Be(100.HitPoints());
    }

    [Fact]
    public void DepletedShellDoesNotBlockDamage()
    {
        var s = hitPoints(100.HitPoints(), DamageShell.Shield, 0.HitPoints());
        testBed.AddComponent(s);
        var h = hitPoints(100.HitPoints(), DamageShell.Health);
        testBed.AddComponent(h);

        doDamage(50.HitPoints());

        s.CurrentHitPoints.Should().Be(0.HitPoints());
        h.CurrentHitPoints.Should().Be(50.HitPoints());
    }

    [Fact]
    public void ShieldReducesDamageAboveThreshold()
    {
        var hp = hitPoints(100.HitPoints(), DamageShell.Shield);
        var s = shield(20.HitPoints(), 0.1);
        testBed.AddComponent(hp);
        testBed.AddComponent(s);

        doDamage(30.HitPoints());

        // 20 hp at 100%; 10 hp at 10%; total 21 damage
        hp.CurrentHitPoints.Should().Be(79.HitPoints());
    }

    [Fact]
    public void ShieldReducesNoDamageIfAllBelowThreshold()
    {
        var hp = hitPoints(100.HitPoints(), DamageShell.Shield);
        var s = shield(20.HitPoints(), 0.1);
        testBed.AddComponent(hp);
        testBed.AddComponent(s);

        doDamage(10.HitPoints());

        hp.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void ArmorReducesDamageBelowThreshold()
    {
        var hp = hitPoints(100.HitPoints(), DamageShell.Armor);
        var a = armor(20.HitPoints(), 0.1);
        testBed.AddComponent(hp);
        testBed.AddComponent(a);

        doDamage(30.HitPoints());

        // 20 hp at 10% at 10%; 10 hp at 100%; total 12 damage
        hp.CurrentHitPoints.Should().Be(88.HitPoints());
    }

    [Fact]
    public void ArmorReducesAllDamageIfAllBelowThreshold()
    {
        var hp = hitPoints(100.HitPoints(), DamageShell.Armor);
        var a = armor(20.HitPoints(), 0.1);
        testBed.AddComponent(hp);
        testBed.AddComponent(a);

        doDamage(10.HitPoints());

        hp.CurrentHitPoints.Should().Be(99.HitPoints());
    }

    [Fact]
    public void HealthAccountsForElementalResistance()
    {
        var hp = hitPoints(100.HitPoints(), DamageShell.Health);
        testBed.AddComponent(hp);
        testBed.AddComponent(resistance(DamageType.Fire, new Resistance(0.3f)));

        doDamage(10.HitPoints(), DamageType.Fire);

        hp.CurrentHitPoints.Should().Be(93.HitPoints());
    }

    [Fact]
    public void HealthIgnoresResistanceForDifferentElement()
    {
        var hp = hitPoints(100.HitPoints(), DamageShell.Health);
        testBed.AddComponent(hp);
        testBed.AddComponent(resistance(DamageType.Lightning, new Resistance(0.3f)));

        doDamage(10.HitPoints(), DamageType.Fire);

        hp.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void ArmorIgnoresElementalResistance()
    {
        var a = hitPoints(100.HitPoints(), DamageShell.Armor);
        testBed.AddComponent(a);
        testBed.AddComponent(resistance(DamageType.Fire, new Resistance(0.3f)));

        doDamage(10.HitPoints(), DamageType.Fire);

        a.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void ShieldIgnoresElementalResistance()
    {
        var s = hitPoints(100.HitPoints(), DamageShell.Shield);
        testBed.AddComponent(s);
        testBed.AddComponent(resistance(DamageType.Fire, new Resistance(0.3f)));

        doDamage(10.HitPoints(), DamageType.Fire);

        s.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void ArmorIsPiercedByLightningDamage()
    {
        var armorShell = hitPoints(100.HitPoints(), DamageShell.Armor);
        var healthShell = hitPoints(100.HitPoints(), DamageShell.Health);
        var a = armor(4.HitPoints(), lightningPiercing: 0.75, blockedEffectiveness: 0);
        testBed.AddComponent(armorShell);
        testBed.AddComponent(healthShell);
        testBed.AddComponent(a);

        doDamage(20.HitPoints(), DamageType.Lightning);

        armorShell.CurrentHitPoints.Should().Be(99.HitPoints());
        healthShell.CurrentHitPoints.Should().Be(85.HitPoints());
    }

    private void doDamage(HitPoints amount, DamageType type = DamageType.Kinetic)
    {
        healthEventReceiver.Damage(new TypedDamage(amount, type), Hit.FromSelf(), null);
    }

    private static HitPointsPool hitPoints(HitPoints amount, DamageShell shell, HitPoints? initialHitPoints = null)
    {
        return new HitPointsPool(new HitPointsPoolParametersTemplate(amount, initialHitPoints, shell, null));
    }

    private static Armor armor(HitPoints threshold, double blockedEffectiveness = 0, double lightningPiercing = 0.5)
    {
        return new Armor(new ArmorParametersTemplate(threshold, blockedEffectiveness, lightningPiercing));
    }

    private static Shield shield(HitPoints threshold, double blockedEffectiveness = 0)
    {
        return new Shield(new ShieldParametersTemplate(threshold, blockedEffectiveness));
    }

    private static DamageResistances resistance(DamageType damageType, Resistance amount)
    {
        return new DamageResistances(
            ImmutableDictionary.CreateRange(
                new Dictionary<DamageType, Resistance> { { damageType, amount } }));
    }
}
