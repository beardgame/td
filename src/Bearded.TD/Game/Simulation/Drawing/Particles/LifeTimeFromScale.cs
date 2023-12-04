using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using static Bearded.TD.Game.Simulation.Drawing.Particles.ParticleSpawning;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesLifeTimeFromScale")]
sealed class LifeTimeFromScale : ParticleUpdater<LifeTimeFromScale.IParameters>
{
    private IProperty<Scale> scale = null!;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        TimeSpan Factor { get; }
        float Noise { get; }
    }

    public LifeTimeFromScale(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();
        ComponentDependencies.Depend<Particles>(Owner, Events,
            p => p.AddExtension(new NotificationOnlyParticleExtension(onNewParticles)));
        ComponentDependencies.Depend<IProperty<Scale>>(Owner, Events, s => scale = s);
    }

    public override void Update(TimeSpan elapsedTime) { }

    private void onNewParticles(int index, int count)
    {
        var now = Owner.Game.Time;
        var timeOffset = scale.Value.Value * Parameters.Factor;

        foreach (ref var particle in Particles.MutableParticles.Slice(index, count))
        {
            particle.TimeOfDeath ??= now;
            particle.TimeOfDeath += timeOffset * Noise(Parameters.Noise);
        }
    }
}

