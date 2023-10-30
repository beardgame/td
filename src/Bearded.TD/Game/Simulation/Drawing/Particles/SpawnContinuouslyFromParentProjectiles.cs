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
    private readonly record struct Emitter(GameObject Object, IMoving? Velocity, Instant NextSpawn);

    private readonly List<Emitter> emitters = new();

    public interface IParameters : IParametersTemplate<IParameters>, IParticleSpawnParameters
    {
        [Modifiable(1)]
        TimeSpan Interval { get; }
        float IntervalNoise { get; }

        bool RandomStartInterval { get; }
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
        var nextSpawn = Owner.Game.Time;
        if (Parameters.RandomStartInterval)
            nextSpawn += Parameters.Interval * StaticRandom.Float();

        e.Projectile.TryGetSingleComponent<IMoving>(out var velocity);

        emitters.Add(new Emitter(e.Projectile, velocity, nextSpawn));
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

            spawnFrom(emitter);
            emitters[i] = emitter with { NextSpawn = now + Parameters.Interval * Noise(Parameters.IntervalNoise) };
        }
    }

    private void spawnFrom(Emitter emitter)
    {
        var source = emitter.Object;
        Particles.CreateParticles(
            Parameters,
            emitter.Velocity?.Velocity ?? Velocity3.Zero,
            source.Direction,
            Owner.Game.Time,
            source.Position);
    }
}

