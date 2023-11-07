using Bearded.Graphics;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesOverrideColorOfLast")]
sealed class OverrideColorOfLast : ParticleUpdater<OverrideColorOfLast.IParameters>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        Color Color { get; }
    }

    public OverrideColorOfLast(IParameters parameters) : base(parameters)
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Particles.Count == 0)
            return;

        Particles.MutableParticles[^1].Color = Parameters.Color;
    }
}

