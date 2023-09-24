using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesDrawConnected")]
sealed class DrawConnected : ParticleUpdater<DrawConnected.IParameters>, IListener<DrawComponents>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISpriteBlueprint Sprite { get; }
        Shader? Shader { get; }
        SpriteDrawGroup? DrawGroup { get; }
        int DrawGroupOrderKey { get; }
    }

    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    public DrawConnected(IParameters parameters) : base(parameters)
    {
    }

    public override void Activate()
    {
        base.Activate();

        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite, Parameters.Shader,
            Parameters.DrawGroup ?? SpriteDrawGroup.Particle, Parameters.DrawGroupOrderKey);

        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(DrawComponents e)
    {
        var particles = Particles.ImmutableParticles;
        if (particles.Length < 2)
            return;

        var drawer = e.Drawer;
        var previous = particles[0];
        var current = particles[1];
        var previousNormal = normalBetween(previous, current);

        for (var i = 2; i < particles.Length; i++)
        {
            var next = particles[i];
            var currentNormal = normalBetween(previous, next);

            drawer.DrawQuad(
                sprite,
                previous.Position.NumericValue - (previousNormal * previous.Size).WithZ(),
                previous.Position.NumericValue + (previousNormal * previous.Size).WithZ(),
                current.Position.NumericValue + (currentNormal * current.Size).WithZ(),
                current.Position.NumericValue - (currentNormal * current.Size).WithZ(),
                previous.Color
                );

            previous = current;
            previousNormal = currentNormal;
            current = next;
        }

        var lastNormal = normalBetween(previous, current);
        drawer.DrawQuad(
            sprite,
            previous.Position.NumericValue - (previousNormal * previous.Size).WithZ(),
            previous.Position.NumericValue + (previousNormal * previous.Size).WithZ(),
            current.Position.NumericValue + (lastNormal * current.Size).WithZ(),
            current.Position.NumericValue - (lastNormal * current.Size).WithZ(),
            previous.Color
        );
    }

    private static Vector2 normalBetween(in Particle p1, in Particle p2)
        => (p2.Position - p1.Position).NumericValue.Xy.PerpendicularRight.NormalizedSafe() * 0.5f;
}

