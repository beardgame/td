using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesDrag")]
sealed class ParticlesDrag : ParticleUpdater<ParticlesDrag.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        float? Linear { get; }
        float? Angular { get; }
    }

    public ParticlesDrag(IParameters parameters) : base(parameters)
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Parameters.Linear is { } linear)
        {
            var f = MathF.Exp(-linear * (float)elapsedTime.NumericValue);

            foreach (ref var p in Particles.MutableParticles)
            {
                p.Velocity *= f;
            }
        }

        if (Parameters.Angular is { } angular)
        {
            var f = MathF.Exp(-angular * (float)elapsedTime.NumericValue);

            foreach (ref var p in Particles.MutableParticles)
            {
                p.AngularVelocity *= f;
            }
        }
    }
}

