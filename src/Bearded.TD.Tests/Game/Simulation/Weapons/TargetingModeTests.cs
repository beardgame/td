using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Simulation.Weapons;

public sealed class TargetingModeTests
{
    [Fact]
    public void ArbitraryReturnsEnemy()
    {
        var obj1 = gameObject();
        var obj2 = gameObject();

        var chosen = TargetingMode.Arbitrary.SelectTarget(ImmutableArray.Create(obj1, obj2), targetingContext());

        chosen.Should().BeOfType<GameObject>();
    }

    [Fact]
    public void ArbitraryReturnsNoEnemyWhenNoCandidates()
    {
        var chosen = TargetingMode.Arbitrary.SelectTarget(ImmutableArray<GameObject>.Empty, targetingContext());

        chosen.Should().BeNull();
    }

    [Fact]
    public void LeastRotationReturnsEnemyWithLeastRotation()
    {
        var weaponPos = new Position3(2, 5, 1);
        var obj1 = gameObjectInDirection(weaponPos, Direction2.FromDegrees(20));
        var obj2 = gameObjectInDirection(weaponPos, Direction2.FromDegrees(180));

        var chosen = TargetingMode.LeastRotation.SelectTarget(
            ImmutableArray.Create(obj1, obj2),
            targetingContext(weaponPos, Direction2.FromDegrees(45)));

        chosen.Should().Be(obj1);
    }

    [Fact]
    public void LeastRotationReturnsSingleEnemy()
    {
        var obj1 = gameObjectInDirection(Position3.Zero, Direction2.FromDegrees(20));

        var chosen = TargetingMode.LeastRotation.SelectTarget(ImmutableArray.Create(obj1), targetingContext());

        chosen.Should().Be(obj1);
    }

    [Fact]
    public void LeastRotationReturnsNoEnemyWhenNoCandidates()
    {
        var chosen = TargetingMode.LeastRotation.SelectTarget(ImmutableArray<GameObject>.Empty, targetingContext());

        chosen.Should().BeNull();
    }

    [Fact]
    public void LeastRotationReturnsEnemyIfWeaponNoDirection()
    {
        var weaponPos = new Position3(2, 5, 1);
        var obj1 = gameObjectInDirection(weaponPos, Direction2.FromDegrees(20));
        var obj2 = gameObjectInDirection(weaponPos, Direction2.FromDegrees(180));

        var chosen = TargetingMode.LeastRotation.SelectTarget(
            ImmutableArray.Create(obj1, obj2),
            targetingContext(weaponPos, dir: null));

        chosen.Should().BeOfType<GameObject>();
    }

    [Fact]
    public void HighestHealthModeReturnsEnemyWithHighestHealth()
    {
        var obj1 = gameObjectWithHealth(100.HitPoints());
        var obj2 = gameObjectWithHealth(200.HitPoints());

        var chosen = TargetingMode.HighestHealth.SelectTarget(ImmutableArray.Create(obj1, obj2), targetingContext());

        chosen.Should().Be(obj2);
    }

    [Fact]
    public void HighestHealthModeReturnsSingleEnemy()
    {
        var obj1 = gameObjectWithHealth(100.HitPoints());

        var chosen = TargetingMode.HighestHealth.SelectTarget(ImmutableArray.Create(obj1), targetingContext());

        chosen.Should().Be(obj1);
    }

    [Fact]
    public void HighestHealthModeReturnsNoEnemyWhenNoCandidates()
    {
        var chosen = TargetingMode.HighestHealth.SelectTarget(ImmutableArray<GameObject>.Empty, targetingContext());

        chosen.Should().BeNull();
    }

    [Fact]
    public void HighestHealthModeReturnsHealthBeforeNoHealthEnemy()
    {
        var obj1 = gameObject();
        var obj2 = gameObjectWithHealth(100.HitPoints());

        var chosen = TargetingMode.HighestHealth.SelectTarget(ImmutableArray.Create(obj1, obj2), targetingContext());

        chosen.Should().Be(obj2);
    }

    [Fact]
    public void HighestHealthModeReturnsNoHealthEnemyIfNoOther()
    {
        var obj1 = gameObject();

        var chosen = TargetingMode.HighestHealth.SelectTarget(ImmutableArray.Create(obj1), targetingContext());

        chosen.Should().Be(obj1);
    }

    [Fact]
    public void LowestHealthModeReturnsEnemyWithLowestHealth()
    {
        var obj1 = gameObjectWithHealth(100.HitPoints());
        var obj2 = gameObjectWithHealth(200.HitPoints());

        var chosen = TargetingMode.LowestHealth.SelectTarget(ImmutableArray.Create(obj1, obj2), targetingContext());

        chosen.Should().Be(obj1);
    }

    [Fact]
    public void LowestHealthModeReturnsSingleEnemy()
    {
        var obj1 = gameObjectWithHealth(100.HitPoints());

        var chosen = TargetingMode.LowestHealth.SelectTarget(ImmutableArray.Create(obj1), targetingContext());

        chosen.Should().Be(obj1);
    }

    [Fact]
    public void LowestHealthModeReturnsNoEnemy()
    {
        var chosen = TargetingMode.LowestHealth.SelectTarget(ImmutableArray<GameObject>.Empty, targetingContext());

        chosen.Should().BeNull();
    }

    [Fact]
    public void LowestHealthModeReturnsHealthBeforeNoHealthEnemy()
    {
        var obj1 = gameObject();
        var obj2 = gameObjectWithHealth(100.HitPoints());

        var chosen = TargetingMode.LowestHealth.SelectTarget(ImmutableArray.Create(obj1, obj2), targetingContext());

        chosen.Should().Be(obj2);
    }

    [Fact]
    public void LowestHealthModeReturnsNoHealthEnemyIfNoOther()
    {
        var obj1 = gameObject();

        var chosen = TargetingMode.LowestHealth.SelectTarget(ImmutableArray.Create(obj1), targetingContext());

        chosen.Should().Be(obj1);
    }

    private static TargetingContext targetingContext() => targetingContext(Position3.Zero, Direction2.Zero);

    private static TargetingContext targetingContext(Position3 pos, Direction2? dir) => new(pos, dir, null!);

    private static GameObject gameObject() => gameObjectWithPosition(Position3.Zero);

    private static GameObject gameObjectInDirection(Position3 pos, Direction2 dir) =>
        gameObjectWithPosition(pos + new Difference3(dir.Vector.WithZ()));

    private static GameObject gameObjectWithPosition(Position3 pos) => new(null, pos, Direction2.Zero);

    private static GameObject gameObjectWithHealth(HitPoints hp)
    {
        var obj = new GameObject(null, Position3.Zero, Direction2.Zero);
        obj.AddComponent(new Health(new HealthParametersTemplate(HitPoints.Max, hp)));
        return obj;
    }
}
