﻿using System;
using Bearded.Graphics;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

static class ParticleSpawning
{
    public static Span<Particle> CreateParticles(
        this Particles particles,
        IParticleSpawnParameters parameters,
        Velocity3 sharedVelocity,
        Direction2 baseDirection,
        Instant now,
        Position3 here,
        int? countOverride = null)
    {
        var direction = baseDirection + parameters.Orientation;
        var color = parameters.Color ?? Color.White;
        var count = countOverride ?? parameters.Count;
        var particleSpan = particles.AddParticles(count);

        var unitX = Vector2.UnitX;
        var unitY = Vector2.UnitY;

        if (parameters.RelativeToDirection)
        {
            unitY = baseDirection.Vector;
            unitX = unitY.PerpendicularRight;
        }

        for (var i = 0; i < particleSpan.Length; i++)
        {
            var velocity = sharedVelocity
                + parameters.Velocity * noise(parameters.VelocityNoise)
                + Vectors.GetRandomUnitVector3() * parameters.RandomVelocity * noise(parameters.RandomVelocityNoise);

            velocity = (velocity.X * unitY + velocity.Y * unitX).WithZ(velocity.Z);

            var offset = parameters.Offset;
            offset = (offset.X * unitY + offset.Y * unitX).WithZ(offset.Z);

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
