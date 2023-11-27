using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesOffsetLifeTime")]
sealed class OffsetLifeTime : ParticleUpdater<OffsetLifeTime.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        float ByVelocity { get; }
        TimeSpan Constant { get; }
    }

    public OffsetLifeTime(IParameters parameters) : base(parameters)
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
        foreach (ref var particle in Particles.MutableParticles.Slice(index, count))
        {
            var offset = particle.Velocity.Length.NumericValue * Parameters.ByVelocity;

            var offsetTime = new TimeSpan(offset) + Parameters.Constant;

            particle.CreationTime += offsetTime;
            particle.TimeOfDeath += offsetTime;

            Owner.Game.Meta.Logger.Debug?.Log(offsetTime);
        }
    }
}
