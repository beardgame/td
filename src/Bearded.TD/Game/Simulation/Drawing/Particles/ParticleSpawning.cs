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
        var direction = baseDirection + parameters.Orientation;
        var color = parameters.Color ?? Color.White;
        var count = countOverride ?? parameters.Count;
        var particleSpan = particles.AddParticles(count);

        var velocityUnitX = Vector2.UnitX;
        var velocityUnitY = Vector2.UnitY;

        if (parameters.VelocityRelativeToDirection)
        {
            velocityUnitY = -direction.Vector;
            velocityUnitX = velocityUnitY.PerpendicularRight;
        }

        for (var i = 0; i < particleSpan.Length; i++)
        {
            var velocity = sharedVelocity
                + parameters.Velocity * noise(parameters.VelocityNoise)
                + Vectors.GetRandomUnitVector3() * parameters.RandomVelocity * noise(parameters.RandomVelocityNoise);

            velocity = (velocity.X * velocityUnitY + velocity.Y * velocityUnitX).WithZ(velocity.Z);

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

    public static float Noise(float amount) => noise(amount);

    private static float noise(float amount)
        => amount == 0 ? 1 : 1 + StaticRandom.Float(-amount, amount);
}
