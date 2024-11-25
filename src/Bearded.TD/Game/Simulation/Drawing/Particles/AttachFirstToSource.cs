using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesAttachFirstToSource")]
sealed class AttachFirstToSource : ParticleUpdater
{
    private IPositionable? source;

    public override void Activate()
    {
        base.Activate();

        ComponentDependencies.Depend<IProperty<Source>>(Owner, Events, s => source = s.Value.Object);
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
    }
}
