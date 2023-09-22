using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesSpawnOnCreate")]
sealed class SpawnOnCreate : ParticleUpdater<SpawnOnCreate.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>, IParticleSpawnParameters
    {
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

        var baseDirection = Owner.Direction;
        var vectorVelocity = Velocity3.Zero;

        if (Owner.TryGetSingleComponent<IDirected3>(out var directed))
        {
            baseDirection = Direction2.Of(directed.Direction.XY().NumericValue);
            vectorVelocity = directed.Direction.NumericValue.NormalizedSafe() * Parameters.VectorVelocity;
        }

        var sharedVelocity = reflectionVelocity + incidentVelocity + vectorVelocity;

        var count = scale == null ? Parameters.Count : (int) (Parameters.Count * scale.Value.Value);

        Particles.CreateParticles(Parameters, sharedVelocity, baseDirection, Owner.Game.Time, Owner.Position, count);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

