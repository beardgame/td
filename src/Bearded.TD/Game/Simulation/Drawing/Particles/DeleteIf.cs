using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesDeleteIf")]
sealed class DeleteIf : ParticleUpdater<DeleteIf.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        bool ParentIsDeleted { get; }
        bool NoParticles { get; }
        bool NoAliveParticles { get; }
    }

    public DeleteIf(IParameters parameters) : base(parameters)
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Parameters.ParentIsDeleted && hasAliveParent)
            return;

        if (Parameters.NoParticles && Particles.Count > 0)
            return;

        if (Parameters.NoAliveParticles && hasAliveParticles())
            return;

        Owner.Delete();
    }

    private bool hasAliveParticles()
    {
        var now = Owner.Game.Time;
        foreach (var particle in Particles.ImmutableParticles)
        {
            if (particle.IsAliveAtTime(now))
                return true;
        }

        return false;
    }

    private bool hasAliveParent => Owner.Parent is { Deleted: false } parent;
}

