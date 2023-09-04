using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
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
            foreach (ref var p in Particles.MutableParticles)
            {
                var dragForce = p.Velocity.LengthSquared.NumericValue * linear;
                var direction = p.Velocity.NumericValue.NormalizedSafe();

                var dragAcceleration = new Acceleration3(direction * -dragForce);
                p.Velocity += dragAcceleration * elapsedTime;
            }

        if (Parameters.Angular is { } angular)
            foreach (ref var p in Particles.MutableParticles)
            {
                var dragForce = p.AngularVelocity.NumericValue * angular;
                var direction = Math.Abs(p.AngularVelocity.NumericValue);

                var dragAcceleration = AngularAcceleration.FromRadians(direction * -dragForce);
                p.AngularVelocity += dragAcceleration * elapsedTime;
            }
    }
}

