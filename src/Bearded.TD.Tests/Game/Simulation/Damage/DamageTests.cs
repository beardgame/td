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
    public void DamageReducesHealth()
    {
        var h = health(100.HitPoints());
        testBed.AddComponent(h);

        doDamage(10.HitPoints());

        h.CurrentHitPoints.Should().Be(90.HitPoints());
        h.CurrentHealth.Should().Be(90.HitPoints());
    }

    [Fact]
    public void DamageReducesArmor()
    {
        var a = armor(100.HitPoints(), 0.HitPoints());
        testBed.AddComponent(a);

        doDamage(10.HitPoints());

        a.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void DamageReducesShield()
    {
        var s = shield(100.HitPoints(), 100.HitPoints());
        testBed.AddComponent(s);

        doDamage(10.HitPoints());

        s.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void DamageReducesShieldBeforeArmor()
    {
        var a = armor(100.HitPoints(), 0.HitPoints());
        testBed.AddComponent(a);
        var s = shield(100.HitPoints(), 100.HitPoints());
        testBed.AddComponent(s);

        doDamage(10.HitPoints());

        a.CurrentHitPoints.Should().Be(100.HitPoints());
        s.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void DamageReducesArmorBeforeHealth()
    {
        var a = armor(100.HitPoints(), 0.HitPoints());
        testBed.AddComponent(a);
        var h = health(100.HitPoints());
        testBed.AddComponent(h);

        doDamage(10.HitPoints());

        a.CurrentHitPoints.Should().Be(90.HitPoints());
        h.CurrentHitPoints.Should().Be(100.HitPoints());
    }

    [Fact]
    public void DamageToShieldDoesNotOverflow()
    {
        var s = shield(100.HitPoints(), 100.HitPoints());
        testBed.AddComponent(s);
        var h = health(100.HitPoints());
        testBed.AddComponent(h);

        doDamage(150.HitPoints());

        s.CurrentHitPoints.Should().Be(0.HitPoints());
        h.CurrentHitPoints.Should().Be(100.HitPoints());
    }

    [Fact]
    public void DamageToArmorDoesNotOverflow()
    {
        var a = armor(100.HitPoints(), 0.HitPoints());
        testBed.AddComponent(a);
        var h = health(100.HitPoints());
        testBed.AddComponent(h);

        doDamage(150.HitPoints());

        a.CurrentHitPoints.Should().Be(0.HitPoints());
        h.CurrentHitPoints.Should().Be(100.HitPoints());
    }

    [Fact]
    public void DepletedShieldDoesNotBlockDamage()
    {
        var s = shield(0.HitPoints(), 100.HitPoints());
        testBed.AddComponent(s);
        var h = health(100.HitPoints());
        testBed.AddComponent(h);

        doDamage(50.HitPoints());

        s.CurrentHitPoints.Should().Be(0.HitPoints());
        h.CurrentHitPoints.Should().Be(50.HitPoints());
    }

    [Fact]
    public void DepletedArmorDoesNotBlockDamage()
    {
        var a = armor(0.HitPoints(), 0.HitPoints());
        testBed.AddComponent(a);
        var h = health(100.HitPoints());
        testBed.AddComponent(h);

        doDamage(50.HitPoints());

        a.CurrentHitPoints.Should().Be(0.HitPoints());
        h.CurrentHitPoints.Should().Be(50.HitPoints());
    }

    [Fact]
    public void ShieldReducesDamageAboveThreshold()
    {
        var s = shield(100.HitPoints(), 20.HitPoints(), 0.1);
        testBed.AddComponent(s);

        doDamage(30.HitPoints());

        // 20 hp at 100%; 10 hp at 10%; total 21 damage
        s.CurrentHitPoints.Should().Be(79.HitPoints());
    }

    [Fact]
    public void ShieldReducesNoDamageIfAllBelowThreshold()
    {
        var s = shield(100.HitPoints(), 20.HitPoints(), 0.1);
        testBed.AddComponent(s);

        doDamage(10.HitPoints());

        s.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void ArmorReducesDamageBelowThreshold()
    {
        var a = armor(100.HitPoints(), 20.HitPoints(), 0.1);
        testBed.AddComponent(a);

        doDamage(30.HitPoints());

        // 20 hp at 10% at 10%; 10 hp at 100%; total 12 damage
        a.CurrentHitPoints.Should().Be(88.HitPoints());
    }

    [Fact]
    public void ArmorReducesAllDamageIfAllBelowThreshold()
    {
        var a = armor(100.HitPoints(), 20.HitPoints(), 0.1);
        testBed.AddComponent(a);

        doDamage(10.HitPoints());

        a.CurrentHitPoints.Should().Be(99.HitPoints());
    }

    [Fact]
    public void HealthAccountsForElementalResistance()
    {
        var h = health(100.HitPoints());
        testBed.AddComponent(h);
        testBed.AddComponent(resistance(DamageType.Fire, new Resistance(0.3f)));

        doDamage(10.HitPoints(), DamageType.Fire);

        h.CurrentHitPoints.Should().Be(93.HitPoints());
    }

    [Fact]
    public void HealthIgnoresResistanceForDifferentElement()
    {
        var h = health(100.HitPoints());
        testBed.AddComponent(h);
        testBed.AddComponent(resistance(DamageType.Lightning, new Resistance(0.3f)));

        doDamage(10.HitPoints(), DamageType.Fire);

        h.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void ArmorIgnoresElementalResistance()
    {
        var a = armor(100.HitPoints(), 0.HitPoints());
        testBed.AddComponent(a);
        testBed.AddComponent(resistance(DamageType.Fire, new Resistance(0.3f)));

        doDamage(10.HitPoints(), DamageType.Fire);

        a.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    [Fact]
    public void ShieldIgnoresElementalResistance()
    {
        var s = shield(100.HitPoints(), 100.HitPoints());
        testBed.AddComponent(s);
        testBed.AddComponent(resistance(DamageType.Fire, new Resistance(0.3f)));

        doDamage(10.HitPoints(), DamageType.Fire);

        s.CurrentHitPoints.Should().Be(90.HitPoints());
    }

    private void doDamage(HitPoints amount, DamageType type = DamageType.Kinetic)
    {
        healthEventReceiver.Damage(new TypedDamage(amount, type), null);
    }

    private static Health health(HitPoints maxHp)
    {
        return new Health(new HealthParametersTemplate(maxHp, null));
    }

    private static Armor armor(HitPoints maxHp, HitPoints threshold, double blockedEffectiveness = 0)
    {
        return new Armor(new ArmorParametersTemplate(maxHp, threshold, blockedEffectiveness));
    }

    private static Shield shield(HitPoints maxHp, HitPoints threshold, double blockedEffectiveness = 0)
    {
        return new Shield(new ShieldParametersTemplate(maxHp, threshold, blockedEffectiveness));
    }

    private static DamageResistances resistance(DamageType damageType, Resistance amount)
    {
        return new DamageResistances(
            ImmutableDictionary.CreateRange(
                new Dictionary<DamageType, Resistance> { { damageType, amount } }));
    }
}
