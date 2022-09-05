using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("animatedSprite")]
class AnimatedSprite : Component<AnimatedSprite.IParameters>, IListener<DrawComponents>
{
    internal enum RepeatMode
    {
        Once = 0,
        Loop,
    }

    internal readonly record struct KeyFrame(
        TimeSpan Duration,
        Unit Size,
        Color Color)
    {
        private static readonly IInterpolationMethod1d interpolation = Interpolation1d.Linear;

        public static KeyFrame Interpolate(KeyFrame a, KeyFrame b, double t)
        {
            var size = interpolation.Interpolate(a.Size.NumericValue, b.Size.NumericValue, t).U();
            var color = Color.Lerp(a.Color, b.Color, (float)t);

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
        RepeatMode RepeatMode { get; }
        ImmutableArray<KeyFrame> KeyFrames { get; }
    }


    private SpriteDrawInfo<UVColorVertex, Color> sprite;
    private Instant startTime;
    private TimeSpan totalDuration;

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
        totalDuration = Parameters.KeyFrames.Sum(f => f.Duration.NumericValue).S();
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
        var keyframe = getKeyFrame();

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

    private KeyFrame getKeyFrame()
    {
        var time = Owner.Game.Time - startTime;
        if (time >= totalDuration)
        {
            if (Parameters.RepeatMode == RepeatMode.Once)
                return Parameters.KeyFrames.Last();

            if (Parameters.RepeatMode == RepeatMode.Loop)
                time = (time.NumericValue % totalDuration.NumericValue).S();
        }

        var firstFrame = Parameters.KeyFrames.First();
        if (time < firstFrame.Duration)
            return firstFrame;

        for (var i = 1; i < Parameters.KeyFrames.Length; i++)
        {
            time -= firstFrame.Duration;
            var secondFrame = Parameters.KeyFrames[i];

            if (time < secondFrame.Duration)
                return KeyFrame.Interpolate(firstFrame, secondFrame, time / secondFrame.Duration);

            firstFrame = secondFrame;
        }

        return firstFrame;
    }
}
