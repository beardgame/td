using Bearded.Graphics;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.Vectors;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesSpawnOnCreate")]
sealed class SpawnOnCreate : ParticleUpdater<SpawnOnCreate.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
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
        bool StartDisabled { get; }

        Speed VectorVelocity { get; }
        Speed IncidentVelocity { get; }
        Speed ReflectionVelocity { get; }
    }

    public SpawnOnCreate(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();

        createParticles();
    }

    private void createParticles()
    {
        var hitInfo = Owner.TryGetSingleComponent<IProperty<Impact>>(out var h) ? h : null;
        var scale = Owner.TryGetSingleComponent<IProperty<Scale>>(out var s) ? s : null;

        var reflectionVelocity = hitInfo != null
            ? hitInfo.Value.GetReflection().NumericValue * Parameters.ReflectionVelocity
            : Velocity3.Zero;

        var incidentVelocity = hitInfo != null
            ? hitInfo.Value.IncidentDirection.NumericValue * Parameters.IncidentVelocity
            : Velocity3.Zero;

        var baseDirection = Direction2.Zero + (Parameters.Orientation ?? Angle.Zero);
        var vectorVelocity = Velocity3.Zero;

        if (Owner.TryGetSingleComponent<IDirected3>(out var directed))
        {
            if (Parameters.Orientation == null)
                baseDirection = Direction2.Of(directed.Direction.XY().NumericValue);
            vectorVelocity = directed.Direction.NumericValue.NormalizedSafe() * Parameters.VectorVelocity;
        }

        var sharedVelocity = reflectionVelocity + incidentVelocity + vectorVelocity;

        var color = Parameters.Color ?? Color.White;
        var now = Owner.Game.Time;
        var here = Owner.Position;

        var count = scale == null ? Parameters.Count : (int) (Parameters.Count * scale.Value.Value);

        var particles = Particles.AddParticles(count);

        for (var i = 0; i < particles.Length; i++)
        {
            var velocity = sharedVelocity
                + Parameters.Velocity * noise(Parameters.VelocityNoise)
                + GetRandomUnitVector3() * Parameters.RandomVelocity * noise(Parameters.RandomVelocityNoise);
            var orientation = baseDirection + Angle.FromDegrees(360) * noise(Parameters.OrientationNoise);
            var angularVelocity = Parameters.AngularVelocity * noise(Parameters.AngularVelocityNoise);
            var timeOfDeath = Parameters.LifeTime is { } lifeTime
                ? now + lifeTime * noise(Parameters.LifeTimeNoise)
                : (Instant?)null;
            particles[i] = new Particle{
                Position = here,
                Velocity = velocity,
                Direction = orientation,
                AngularVelocity = angularVelocity,
                Size = Parameters.Size.NumericValue * noise(Parameters.SizeNoise),
                Color = color,
                CreationTime = now,
                TimeOfDeath = timeOfDeath,
            };
        }
    }

    private static float noise(float amount)
        => amount == 0 ? 1 : 1 + StaticRandom.Float(-amount, amount);

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

