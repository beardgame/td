using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("beamEmitter")]
sealed class BeamEmitter : WeaponCycleHandler<BeamEmitter.IParameters>, IListener<DrawComponents>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(10, Type = AttributeType.DamageOverTime)]
        int DamagePerSecond { get; }

        [Modifiable(Type = AttributeType.Range)]
        Unit Range { get; }

        [Modifiable(0.0f, Type = AttributeType.PiercingFactor)]
        float PiercingFactor { get; }

        Color Color { get; }

        [Modifiable(3)]
        Unit Width { get; }

        [Modifiable(1)]
        Unit CoreWidth { get; }
    }

    private readonly record struct BeamSegment(Position3 Start, Position3 End, float DamageFactor);

    private static readonly TimeSpan damageTimeSpan = 0.1.S();
    private const double minDamagePerSecond = .05;

    private Instant? lastDamageTime;
    private bool drawBeam;
    private readonly List<BeamSegment> beamSegments = new();
    private ITargeter<IPositionable>? targeter;

    public BeamEmitter(IParameters parameters)
        : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        base.OnAdded();
        Events.Subscribe(this);

        ComponentDependencies.DependDynamic<ITargeter<IPositionable>>(Owner, Events, c => targeter = c);
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        Events.Unsubscribe(this);
    }

    protected override void UpdateIdle(TimeSpan elapsedTime)
    {
        lastDamageTime = null;
        drawBeam = false;
    }

    protected override void UpdateShooting(TimeSpan elapsedTime)
    {
        lastDamageTime ??= Game.Time;
        drawBeam = true;

        emitBeam();
    }

    private void emitBeam()
    {
        var ray = new Ray3(
            Weapon.Position,
            getBeamDirection()
        );

        var results = Parameters.PiercingFactor > minDamagePerSecond
            ? Game.Level.CastPiercingRayAgainstEnemies(
                ray, Game.UnitLayer, Game.PassabilityManager.GetLayer(Passability.Projectile))
            : Game.Level.CastRayAgainstEnemies(
                ray, Game.UnitLayer, Game.PassabilityManager.GetLayer(Passability.Projectile)).Yield();

        beamSegments.Clear();
        var lastEnd = Weapon.Position;
        var damageFactor = 1.0f;

        var timeSinceLastDamage = Game.Time - lastDamageTime;
        var canDamageThisFrame = timeSinceLastDamage > damageTimeSpan;
        var damagedThisFrame = false;

        foreach (var (type, _, point, enemy, _, _) in results)
        {
            beamSegments.Add(new BeamSegment(lastEnd, point, damageFactor));

            if (!canDamageThisFrame)
                continue;

            if (type == RayCastResultType.HitEnemy)
            {
                _ = enemy ?? throw new InvalidOperationException();
                damagedThisFrame |= tryDamage(enemy, damageFactor);
                damageFactor *= Parameters.PiercingFactor;
            }
        }

        if (damagedThisFrame)
        {
            // TODO: strictly speaking this will lead to slightly too little damage, based on framerate
            // it would be better to store the next time we are allowed to do damage, and add the interval to it
            // accounting for periods where we don't hit anything correctly
            lastDamageTime = Game.Time;
        }
    }

    private Difference3 getBeamDirection()
    {
        return targeter?.Target is { } target
            ? (target.Position - Weapon.Position).NumericValue.NormalizedSafe() * Parameters.Range
            : (Weapon.Direction * Parameters.Range).WithZ();
    }

    private bool tryDamage(IComponentOwner enemy, float damageFactor)
    {
        var adjustedDamagePerSecond = damageFactor * Parameters.DamagePerSecond;

        if (adjustedDamagePerSecond < minDamagePerSecond)
            return false;

        var damage = new TypedDamage(
            StaticRandom.Discretise((float)(adjustedDamagePerSecond * damageTimeSpan.NumericValue)).HitPoints(),
            DamageType.Energy);

        return DamageExecutor.FromObject(Owner).TryDoDamage(enemy, damage);
    }

    public void HandleEvent(DrawComponents e)
    {
        if (!drawBeam)
            return;

        var shapeDrawer = e.Core.ConsoleBackground;
        var baseAlpha = StaticRandom.Float(0.5f, 0.8f);

        foreach (var (start, end, factor) in beamSegments)
        {
            shapeDrawer.DrawLine(
                start.NumericValue,
                end.NumericValue,
                Parameters.Width.NumericValue * PixelSize * 0.5f,
                Parameters.Color.WithAlpha() * baseAlpha * factor);

            shapeDrawer.DrawLine(
                start.NumericValue,
                end.NumericValue,
                Parameters.CoreWidth.NumericValue * PixelSize * 0.5f,
                Color.White.WithAlpha() * baseAlpha * factor);
        }
    }
}
