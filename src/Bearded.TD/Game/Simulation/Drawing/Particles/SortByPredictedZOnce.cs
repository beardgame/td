using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;
using MemoryExtensions = System.MemoryExtensions;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesSortByPredictedZOnce")]
sealed class SortByPredictedZOnce : ParticleUpdater
{
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
        var particles = Particles.MutableParticles.Slice(index, count);

        MemoryExtensions.Sort(particles, byPredictedZ);
    }

    private static int byPredictedZ(Particle a, Particle b)
    {
        var az = a.Position.Z.NumericValue + a.Velocity.Z.NumericValue;
        var bz = b.Position.Z.NumericValue + b.Velocity.Z.NumericValue;
        return az.CompareTo(bz);
    }
}
