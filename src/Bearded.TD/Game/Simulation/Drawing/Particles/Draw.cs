using System;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("particlesDraw")]
sealed class Draw : ParticleUpdater<Draw.IParameters>, IListener<DrawComponents>
{
    public enum DrawMode
    {
        Sprite,
        Line
    }

    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        ISpriteBlueprint Sprite { get; }
        Sprite.ColorMode ColorMode { get; }
        Shader? Shader { get; }
        DrawOrderGroup? DrawGroup { get; }
        int DrawGroupOrderKey { get; }

        DrawMode DrawMode { get; }
        Unit LineWidth { get; }

        bool ReverseOrder { get; }

        Unit HeightOffset { get; }
    }

    public Draw(IParameters parameters) : base(parameters)
    {
    }


    public override void Activate()
    {
        base.Activate();

        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite, Parameters.Shader,
            Parameters.DrawGroup ?? DrawOrderGroup.Particle, Parameters.DrawGroupOrderKey);

        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(DrawComponents e)
    {
        var ps = Particles.ImmutableParticles;

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
        var position = p.Position.XY().WithZ(p.Position.Z + Parameters.HeightOffset);

        switch (Parameters.DrawMode)
        {
            case DrawMode.Sprite:
                e.Drawer.DrawSprite(
                    sprite,
                    position.NumericValue,
                    p.Size,
                    p.Direction,
                    p.Color
                );
                break;
            case DrawMode.Line:
                var v = p.Velocity.NumericValue * p.Size * 0.5f;
                var w = Vector3.Cross(v.NormalizedSafe(), Vector3.UnitZ) * Parameters.LineWidth.NumericValue * 0.5f;
                var c = position.NumericValue;

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

