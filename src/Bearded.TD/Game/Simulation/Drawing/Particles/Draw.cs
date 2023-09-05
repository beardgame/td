using System;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesDraw")]
sealed class Draw : Component<Draw.IParameters>, IListener<DrawComponents>
{
    public enum DrawMode
    {
        Sprite,
        Line
    }

    private Particles particles = null!;
    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISpriteBlueprint Sprite { get; }
        Sprite.ColorMode ColorMode { get; }
        Shader? Shader { get; }
        SpriteDrawGroup? DrawGroup { get; }
        int DrawGroupOrderKey { get; }

        DrawMode DrawMode { get; }
        Unit LineWidth { get; }

        bool ReverseOrder { get; }
    }

    public Draw(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        ComponentDependencies.Depend<Particles>(Owner, Events, p => particles = p);

        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite, Parameters.Shader,
            Parameters.DrawGroup ?? SpriteDrawGroup.Particle, Parameters.DrawGroupOrderKey);

        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(DrawComponents e)
    {
        var ps = particles.ImmutableParticles;

        if (Parameters.ReverseOrder)
        {
            for (var i = ps.Length; i-->0;)
            {
                draw(e, ps[i]);
            }
        }
        else
        {
            foreach (var p in ps)
            {
                draw(e, p);
            }
        }
    }

    private void draw(DrawComponents e, in Particle p)
    {
        switch (Parameters.DrawMode)
        {
            case DrawMode.Sprite:
                e.Drawer.DrawSprite(
                    sprite,
                    p.Position.NumericValue,
                    p.Size,
                    p.Direction.Radians,
                    p.Color
                );
                break;
            case DrawMode.Line:
                var v = p.Velocity.NumericValue * p.Size * 0.5f;
                var w = Vector3.Cross(v.NormalizedSafe(), Vector3.UnitZ) * Parameters.LineWidth.NumericValue * 0.5f;
                var c = p.Position.NumericValue;

                var p0 = c - v + w;
                var p1 = c + v + w;
                var p2 = c + v - w;
                var p3 = c - v - w;

                e.Drawer.DrawQuad(sprite, p0, p1, p2, p3, p.Color);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

