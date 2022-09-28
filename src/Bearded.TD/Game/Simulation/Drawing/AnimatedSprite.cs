using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Drawing.Animation;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("animatedSprite")]
class AnimatedSprite : Component<AnimatedSprite.IParameters>, IListener<DrawComponents>
{
    internal readonly record struct KeyFrame(
        TimeSpan Duration,
        Unit Size,
        Color Color) : IKeyFrame<KeyFrame>
    {
        private static readonly IInterpolationMethod1d interpolation = Interpolation1d.Linear;

        public KeyFrame InterpolateTowards(KeyFrame other, double t)
        {
            var size = interpolation.Interpolate(Size.NumericValue, other.Size.NumericValue, t).U();
            var color = Color.Lerp(Color, other.Color, (float)t);

            return new KeyFrame(default, size, color);
        }
    }

    internal interface IParameters : IParametersTemplate<IParameters>
    {
        ISpriteBlueprint Sprite { get; }
        Shader? Shader { get; }
        SpriteDrawGroup? DrawGroup { get; }
        int DrawGroupOrderKey { get; }
        Unit HeightOffset { get; }
        Difference2 Offset { get; }
        IKeyFrameAnimation<KeyFrame> Animation { get; }
    }

    private SpriteDrawInfo<UVColorVertex, Color> sprite;
    private Instant startTime;

    public AnimatedSprite(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded() {}

    public override void Activate()
    {
        sprite = SpriteDrawInfo.ForUVColor(Owner.Game, Parameters.Sprite, Parameters.Shader,
            Parameters.DrawGroup ?? SpriteDrawGroup.Particle, Parameters.DrawGroupOrderKey);

        Events.Subscribe(this);

        startTime = Owner.Game.Time;
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(DrawComponents e)
    {
        var keyframe = Parameters.Animation.InterpolateFrameAt(Owner.Game.Time - startTime);

        var p = Owner.Position.NumericValue;
        p.Z += Parameters.HeightOffset.NumericValue;

        if (Parameters.Offset is var offset && offset != Difference2.Zero)
        {
            var unitY = Owner.Direction.Vector;

            var o = unitY * offset.Y + unitY.PerpendicularRight * offset.X;

            p.Xy += o.NumericValue;
        }

        e.Drawer.DrawSprite(sprite, p, keyframe.Size.NumericValue, Owner.Direction.Radians, keyframe.Color);
    }
}