using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;
using static ParticleSpawning;

[Component("particlesSpawnContinuouslyFromParentProjectiles")]
sealed class SpawnContinuouslyFromParentProjectiles : ParticleUpdater<SpawnContinuouslyFromParentProjectiles.IParameters>
{
    private readonly record struct Emitter(
        GameObject Object,
        IMoving? Velocity,
        Instant NextSpawn,
        Instant BirthTime,
        bool ConnectToPrevious);

    private readonly List<Emitter> emitters = new();

    public interface IParameters : IParametersTemplate<IParameters>, IParticleSpawnParameters
    {
        [Modifiable(1)]
        TimeSpan Interval { get; }
        float IntervalNoise { get; }
        TimeSpan StartDelay { get; }

        bool RandomStartInterval { get; }
        TimeSpan? BetweenLastTwoProjectilesIfCreatedWithin { get; }
    }

    public SpawnContinuouslyFromParentProjectiles(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        base.Activate();

        if (Owner.Parent is not { } parent)
        {
            Owner.Game.Meta.Logger.Debug?.Log("Expected owner to have parent.");
            return;
        }

        var projectileListener = new EventListener<ShotProjectile>(onShotProjectile);
        parent.AddComponent(projectileListener);
        Owner.Deleting += () => parent.RemoveComponent(projectileListener);
    }

    private void onShotProjectile(ShotProjectile e)
    {
        var now = Owner.Game.Time;
        var nextSpawn = now + Parameters.StartDelay;
        if (Parameters.RandomStartInterval)
            nextSpawn += Parameters.Interval * StaticRandom.Float();

        e.Projectile.TryGetSingleComponent<IMoving>(out var velocity);

        var connect = false;
        if (Parameters.BetweenLastTwoProjectilesIfCreatedWithin is { } connectThreshold
            && emitters.Count > 0)
        {
            var lastEmitterBirth = emitters[^1].BirthTime;
            if (now - lastEmitterBirth < connectThreshold)
            {
                connect = true;
            }
        }

        emitters.Add(new Emitter(e.Projectile, velocity, nextSpawn, now, connect));
    }

    public override void Update(TimeSpan elapsedTime)
    {
        emitters.RemoveAll(e => e.Object.Deleted);

        var now = Owner.Game.Time;

        for (var i = 0; i < emitters.Count; i++)
        {
            var emitter = emitters[i];
            if (emitter.NextSpawn > now)
                continue;

            spawnFrom(i);
            emitters[i] = emitter with { NextSpawn = now + Parameters.Interval * Noise(Parameters.IntervalNoise) };
        }
    }

    private void spawnFrom(int emitterId)
    {
        var emitter = emitters[emitterId];

        var position = emitter.ConnectToPrevious && emitterId > 0
            ? Position3.Lerp(emitter.Object.Position, emitters[emitterId - 1].Object.Position, StaticRandom.Float())
            : emitter.Object.Position;

        Particles.CreateParticles(
            Parameters,
            emitter.Velocity?.Velocity ?? Velocity3.Zero,
            emitter.Object.Direction,
            Owner.Game.Time,
            position);
    }
}

