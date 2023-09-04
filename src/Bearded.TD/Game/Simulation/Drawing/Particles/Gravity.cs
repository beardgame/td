using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesGravity")]
sealed class Gravity : ParticleUpdater<Gravity.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        float Factor { get; }
    }

    public Gravity(IParameters parameters) : base(parameters)
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var force = Constants.Game.Physics.Gravity3 * Parameters.Factor;

        foreach (ref var p in Particles.MutableParticles)
        {
            p.Velocity += force * elapsedTime;
        }
    }
}

