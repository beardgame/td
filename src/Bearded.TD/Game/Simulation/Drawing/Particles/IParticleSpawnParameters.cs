using Bearded.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.Vectors;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

interface IParticleSpawnParameters
{
    [Modifiable(1)]
    int Count { get; }

    TimeSpan? LifeTime { get; }
    float LifeTimeNoise { get; }

    [Modifiable(1)]
    Unit Size { get; }
    float SizeNoise { get; }

    Color? Color { get; }

    Velocity3 Velocity { get; }
    float VelocityNoise { get; }

    Speed RandomVelocity { get; }
    float RandomVelocityNoise { get; }

    Angle? Orientation { get; }
    float OrientationNoise { get; }

    AngularVelocity AngularVelocity { get; }
    float AngularVelocityNoise { get; }
    bool RandomAngularVelocitySign { get; }
}

static class ParticleSpawnExtensions
{
    public static void CreateParticles(
        this Particles particles,
        IParticleSpawnParameters parameters,
        Velocity3 sharedVelocity,
        Direction2 baseDirection,
        Instant now,
        Position3 here,
        int? countOverride = null)
    {
        var direction = baseDirection + (parameters.Orientation ?? Angle.Zero);
        var color = parameters.Color ?? Color.White;
        var count = countOverride ?? parameters.Count;
        var particleSpan = particles.AddParticles(count);

        for (var i = 0; i < particleSpan.Length; i++)
        {
            var velocity = sharedVelocity
                + parameters.Velocity * noise(parameters.VelocityNoise)
                + GetRandomUnitVector3() * parameters.RandomVelocity * noise(parameters.RandomVelocityNoise);

            var orientation = direction + Angle.FromDegrees(360) * noise(parameters.OrientationNoise);

            var angularVelocity = parameters.AngularVelocity * noise(parameters.AngularVelocityNoise);
            if (parameters.RandomAngularVelocitySign)
                angularVelocity *= StaticRandom.Sign();

            var timeOfDeath = parameters.LifeTime is { } lifeTime
                ? now + lifeTime * noise(parameters.LifeTimeNoise)
                : (Instant?)null;

            particleSpan[i] = new Particle
            {
                Position = here,
                Velocity = velocity,
                Direction = orientation,
                AngularVelocity = angularVelocity,
                Size = parameters.Size.NumericValue * noise(parameters.SizeNoise),
                Color = color,
                CreationTime = now,
                TimeOfDeath = timeOfDeath,
            };
        }
    }

    private static float noise(float amount)
        => amount == 0 ? 1 : 1 + StaticRandom.Float(-amount, amount);
}
