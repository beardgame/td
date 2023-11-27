using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesClampVelocity")]
sealed class ClampVelocity : ParticleUpdater<ClampVelocity.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        Speed? MinZ { get; }
        Speed? MaxZ { get; }
    }

    public ClampVelocity(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();
        ComponentDependencies.Depend<Particles>(Owner, Events,
            p => p.AddExtension(new NotificationOnlyParticleExtension(onNewParticles)));
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    private void onNewParticles(int index, int count)
    {
        var minZ = Parameters.MinZ ?? new Speed(float.NegativeInfinity);
        var maxZ = Parameters.MaxZ ?? new Speed(float.PositiveInfinity);

        foreach (ref var particle in Particles.MutableParticles.Slice(index, count))
        {
            var vz = particle.Velocity.Z.NumericValue;
            if (vz < minZ.NumericValue)
                particle.Velocity = particle.Velocity.XY().WithZ(minZ);
            else if (vz > maxZ.NumericValue)
                particle.Velocity = particle.Velocity.XY().WithZ(maxZ);
        }
    }
}

