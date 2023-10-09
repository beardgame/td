using System.ComponentModel;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesDrawLight")]
sealed class DrawLight : ParticleUpdater<DrawLight.IParameters>, IListener<DrawComponents>
{
    public enum AlphaMode
    {
        Constant = 0,
        FromParticleAlpha,
    }

    public interface IParameters : IParametersTemplate<IParameters>
    {
        Color Color { get; }
        Unit Radius { get; }
        [Modifiable(1)]
        float Intensity { get; }
        AlphaMode AlphaMode { get; }
    }

    public DrawLight(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(DrawComponents e)
    {
        var pointLight = e.Core.PointLight;

        foreach (var p in Particles.ImmutableParticles)
        {
            var a = Parameters.AlphaMode switch
            {
                AlphaMode.Constant => 1,
                AlphaMode.FromParticleAlpha => p.Color.A / 255f,
                _ => throw new InvalidEnumArgumentException(),
            };

            pointLight.Draw(
                p.Position.NumericValue,
                Parameters.Radius.NumericValue,
                Parameters.Color * a,
                Parameters.Intensity,
                false
                );
        }
    }
}

