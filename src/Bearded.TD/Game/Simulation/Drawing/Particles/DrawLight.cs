using System;
using System.ComponentModel;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesDrawLight")]
sealed class DrawLight : ParticleUpdater<DrawLight.IParameters>, IListener<DrawComponents>
{
    private IProperty<Scale>? scale;

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
        float AddIntensityFromScale { get; }
    }

    public DrawLight(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();
        Events.Subscribe(this);

        if (Parameters.AddIntensityFromScale != 0)
            ComponentDependencies.Depend<IProperty<Scale>>(Owner, Events, s => scale = s);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(DrawComponents e)
    {
        var pointLight = e.Core.PointLight;

        var scaleIntensity = scale?.Value.Value * Parameters.AddIntensityFromScale ?? 0;
        var intensity = Parameters.Intensity + scaleIntensity;

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
                intensity,
                false
                );
        }
    }
}

