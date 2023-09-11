using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("delayedChainExplosionOnStuck")]
sealed class DelayedChainExplosionOnStuck
    : Component<DelayedChainExplosionOnStuck.IParameters>,
        IDelayedChainExplosion,
        IListener<EnemyGotStuck>,
        IListener<EnemyGotUnstuck>,
        IListener<DrawComponents>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        TimeSpan Delay { get; }

        [Modifiable(5)]
        Speed PropagationSpeed { get; }

        [Modifiable(2.5)]
        Unit PropagationRange { get; }

        UntypedDamage Damage { get; }
        DamageType? DamageType { get; }
        IGameObjectBlueprint? SpawnObject { get; }
    }

    private ScheduledDetonation? scheduledDetonation;
    private bool detonated;

    public DelayedChainExplosionOnStuck(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe<EnemyGotStuck>(this);
        Events.Subscribe<EnemyGotUnstuck>(this);
        Events.Subscribe<DrawComponents>(this);
    }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime)
    {
        if (detonated)
        {
            return;
        }

        if (scheduledDetonation is { } detonation && detonation.Time <= Owner.Game.Time)
        {
            detonate(detonation);
        }
    }

    private void detonate(ScheduledDetonation detonation)
    {
        explode(detonation.Target);
        propagate(detonation.Time);
        die();
        detonated = true;
    }

    private void explode(Tile target)
    {
        var buildings = Owner.Game.BuildingLayer;
        if (!buildings.TryGetMaterializedBuilding(target, out var targetBuilding))
        {
            return;
        }

        var damage = Parameters.Damage.Typed(Parameters.DamageType ?? DamageType.Kinetic);
        var incident = (targetBuilding.Position - Owner.Position).NormalizedSafe();
        var impact = new Impact(targetBuilding.Position, -incident, incident);
        var hit = Hit.FromAreaOfEffect(impact);

        Owner.Sync(DamageGameObject.Command, Owner, targetBuilding, damage, hit);

        if (Parameters.SpawnObject is { } blueprint)
        {
            Owner.Sync(SpawnGameObject.Command, blueprint, Owner, Owner.Position, Owner.Direction);
        }
    }

    private void propagate(Instant time)
    {
        var objectsInRange = AreaOfEffect.FindObjects(Owner.Game, Owner.Position, Parameters.PropagationRange)
            .Select(o => o.GameObject)
            .ToImmutableArray();
        foreach (var obj in objectsInRange)
        {
            if (obj == Owner || !obj.TryGetSingleComponent<IDelayedChainExplosion>(out var chainExplosion))
            {
                continue;
            }

            var distance = (obj.Position - Owner.Position).Length;
            var delay = new TimeSpan(distance.NumericValue / Parameters.PropagationSpeed.NumericValue); // :(
            chainExplosion.ExplodeEarly(time + delay);
        }
    }

    private void die()
    {
        Events.Send(new EnactDeath());
    }

    public void ExplodeEarly(Instant time)
    {
        if (scheduledDetonation is not { } detonation)
        {
            return;
        }

        if (time < detonation.Time)
        {
            detonation = detonation with { Time = time };
        }

        scheduledDetonation = detonation with { LockedIn = true };
    }

    public void HandleEvent(EnemyGotStuck @event)
    {
        scheduledDetonation = new ScheduledDetonation(Owner.Game.Time + Parameters.Delay, @event.IntendedTarget, false);
    }

    public void HandleEvent(EnemyGotUnstuck @event)
    {
        if (scheduledDetonation is not { LockedIn: false })
        {
            return;
        }

        scheduledDetonation = null;
    }

    public void HandleEvent(DrawComponents @event)
    {
        if (scheduledDetonation is not { } detonation)
        {
            return;
        }

        var timeLeft = detonation.Time - Owner.Game.Time;
        var t = timeLeft / Parameters.Delay;
        var frequency = Interpolate.Lerp(1, 3, (float) t);
        var blinkIsVisible = Math.Sin(frequency * MathConstants.TwoPi * Owner.Game.Time.NumericValue) >= 0;

        if (blinkIsVisible)
        {
            @event.Core.Primitives.FillCircle(
                Owner.Position.NumericValue + 0.1f * Vector3.UnitZ, 0.05f, Color.Red, edges: 6);
        }
    }

    private readonly record struct ScheduledDetonation(Instant Time, Tile Target, bool LockedIn);
}

interface IDelayedChainExplosion
{
    void ExplodeEarly(Instant time);
}
