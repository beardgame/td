using Bearded.Graphics;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.Vectors;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesSpawnContinuously")]
sealed class SpawnContinuously : ParticleUpdater<SpawnContinuously.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        TimeSpan Interval { get; }
        float IntervalNoise { get; }

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

        Angle Orientation { get; }
        float OrientationNoise { get; }

        AngularVelocity AngularVelocity { get; }
        float AngularVelocityNoise { get; }

        bool RandomAngularVelocitySign { get; }
    }

    private Instant nextSpawn;

    public SpawnContinuously(IParameters parameters) : base(parameters)
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (nextSpawn <= Owner.Game.Time)
        {
            spawn();
            nextSpawn = Owner.Game.Time + Parameters.Interval * noise(Parameters.IntervalNoise);
        }
    }

    private void spawn()
    {
        var velocity = Parameters.Velocity * noise(Parameters.VelocityNoise);

        if(Parameters.RandomVelocity != Speed.Zero)
            velocity += Parameters.RandomVelocity * noise(Parameters.RandomVelocityNoise) * GetRandomUnitVector3();

        var timeOfDeath = Parameters.LifeTime is { } lifeTime
            ? Owner.Game.Time + lifeTime * noise(Parameters.LifeTimeNoise)
            : (Instant?)null;

        var orientation = Owner.Direction + Parameters.Orientation +
            Angle.FromDegrees(360) * noise(Parameters.OrientationNoise);

        var angularVelocity = Parameters.AngularVelocity * noise(Parameters.AngularVelocityNoise);

        if (Parameters.RandomAngularVelocitySign)
            angularVelocity *= StaticRandom.Sign();

        var particle = new Particle
        {
            Position = Owner.Position,
            Velocity = velocity,
            Direction = orientation,
            AngularVelocity = angularVelocity,
            Size = Parameters.Size.NumericValue * noise(Parameters.SizeNoise),
            Color = Parameters.Color ?? Color.White,
            CreationTime = Owner.Game.Time,
            TimeOfDeath = timeOfDeath,
        };

        Particles.AddParticle(particle);
    }

    private static float noise(float amount)
        => amount == 0 ? 1 : 1 + StaticRandom.Float(-amount, amount);
}

