using Bearded.TD.Game.Simulation.GameObjects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesMoveLinear")]
sealed class MoveLinear : ParticleUpdater
{
    public override void Update(TimeSpan elapsedTime)
    {
        foreach (ref var p in Particles.MutableParticles)
        {
            p.Position += p.Velocity * elapsedTime;
            p.Direction += p.AngularVelocity * elapsedTime;
        }
    }
}

