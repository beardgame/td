using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesAttachToSourceAndTarget")]
sealed class AttachToSourceAndTarget : ParticleUpdater
{
    private IPositionable? source;
    private IPositionable? target;

    public override void Activate()
    {
        base.Activate();

        ComponentDependencies.Depend<IProperty<Source>>(Owner, Events, s => source = s.Value.Object);
        ComponentDependencies.Depend<IProperty<Target>>(Owner, Events, t => target = t.Value.Object);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var particles = Particles.MutableParticles;
        if (particles.Length == 0)
            return;
        if (source != null)
        {
            particles[0].Position = source.Position;
        }
        if (target != null)
        {
            particles[^1].Position = target.Position;
        }
    }
}

