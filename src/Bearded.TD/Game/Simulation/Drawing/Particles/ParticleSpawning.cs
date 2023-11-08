using System;
using Bearded.Graphics;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

static class ParticleSpawning
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
        CreateParticles(particles, parameters, sharedVelocity, baseDirection, now, here, out var transaction, countOverride);
        transaction.Commit();
    }

    public static Span<Particle> CreateParticles(
        this Particles particles,
        IParticleSpawnParameters parameters,
        Velocity3 sharedVelocity,
        Direction2 baseDirection,
        Instant now,
        Position3 here,
        out Particles.AddTransaction transaction,
        int? countOverride = null)
    {
        sharedVelocity *= parameters.InheritVelocity;
        var direction = baseDirection + parameters.Orientation;
        var color = parameters.Color ?? Color.White;
        var count = countOverride ?? parameters.Count;
        var particleSpan = particles.AddParticles(count, out transaction);

        var unitX = Vector2.UnitX;
        var unitY = Vector2.UnitY;

        if (parameters.RelativeToDirection)
        {
            unitX = baseDirection.Vector;
            unitY = unitX.PerpendicularRight;
        }

        for (var i = 0; i < particleSpan.Length; i++)
        {
            var velocity = sharedVelocity * noise(parameters.InheritVelocityNoise)
                + Vectors.GetRandomUnitVector3() * parameters.RandomVelocity * noise(parameters.RandomVelocityNoise);

            var localVelocity = parameters.Velocity * noise(parameters.VelocityNoise);
            localVelocity = (localVelocity.X * unitX + localVelocity.Y * unitY).WithZ(localVelocity.Z);

            velocity += localVelocity;

            var offset = parameters.Offset * noise(parameters.OffsetNoise);
            offset = (offset.X * unitX + offset.Y * unitY).WithZ(offset.Z);

            if (parameters.RandomOffset != 0.U())
                offset += Vectors.GetRandomUnitVector3() * parameters.RandomOffset * noise(parameters.RandomOffsetNoise);

            var orientation = direction + Angle.FromDegrees(360) * noise(parameters.OrientationNoise);

            var angularVelocity = parameters.AngularVelocity * noise(parameters.AngularVelocityNoise);
            if (parameters.RandomAngularVelocitySign)
                angularVelocity *= StaticRandom.Sign();

            var timeOfDeath = parameters.LifeTime is { } lifeTime
                ? now + lifeTime * noise(parameters.LifeTimeNoise)
                : (Instant?)null;

            particleSpan[i] = new Particle
            {
                Position = here + offset,
                Velocity = velocity,
                Direction = orientation,
                AngularVelocity = angularVelocity,
                Size = parameters.Size.NumericValue * noise(parameters.SizeNoise),
                Color = color,
                CreationTime = now,
                TimeOfDeath = timeOfDeath,
            };
        }

        return particleSpan;
    }

    public static float Noise(float amount) => noise(amount);

    private static float noise(float amount)
        => amount == 0 ? 1 : 1 + StaticRandom.Float(-amount, amount);
}
