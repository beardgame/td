using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Drawing.Particles.OverrideLifeTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesOverrideLifeTime")]
sealed class OverrideLifeTime(IParameters parameters) : ParticleUpdater<IParameters>(parameters)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        bool ResetCreationTime { get; }
        TimeSpan LifeTime { get; }
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
        var now = Owner.Game.Time;

        foreach (ref var particle in Particles.MutableParticles.Slice(index, count))
        {
            if (Parameters.ResetCreationTime)
                particle.CreationTime = now;
            particle.TimeOfDeath = now + Parameters.LifeTime;
        }
    }
}
