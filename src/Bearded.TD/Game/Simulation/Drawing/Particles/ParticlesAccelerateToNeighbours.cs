using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Utilities;
using static Bearded.TD.Game.Simulation.Drawing.Particles.ParticlesSpringBetweenNeighbours;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesSpringBetweenNeighbours")]
sealed class ParticlesSpringBetweenNeighbours(IParameters parameters) : ParticleUpdater<IParameters>(parameters)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        float Stiffness { get; }
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var particles = Particles.MutableParticles;

        for (var i = 1; i < particles.Length; i++)
        {
            ref var previous = ref particles[i - 1];
            ref var current = ref particles[i];

            var difference = current.Position - previous.Position;

            var acceleration = difference * Parameters.Stiffness / 1.S() / 1.S();

            current.Velocity -= acceleration * elapsedTime;
            previous.Velocity += acceleration * elapsedTime;
        }
    }
}
