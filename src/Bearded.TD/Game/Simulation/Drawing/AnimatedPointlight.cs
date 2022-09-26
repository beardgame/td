using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Drawing.Animation;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("animatedPointLight")]
sealed class AnimatedPointLight : Component<AnimatedPointLight.IParameters>, IListener<DrawComponents>
{
    private Instant startTime;

    internal readonly record struct KeyFrame(
        TimeSpan Duration,
        Color Color,
        Unit Radius,
        Unit Height) : IKeyFrame<KeyFrame>
    {
        private static readonly IInterpolationMethod1d interpolation = Interpolation1d.Linear;

        public KeyFrame InterpolateTowards(KeyFrame other, double t)
        {
            var color = Color.Lerp(Color, other.Color, (float)t);
            var radius = interpolation.Interpolate(Radius.NumericValue, other.Radius.NumericValue, t).U();
            var height = interpolation.Interpolate(Height.NumericValue, other.Height.NumericValue, t).U();
            
            return new KeyFrame(default, color, radius, height);
        }
    }

    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IKeyFrameAnimation<KeyFrame> Animation { get; }
    }

    public AnimatedPointLight(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Activate()
    {
        startTime = Owner.Game.Time;
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(DrawComponents e)
    {
        var keyFrame = Parameters.Animation.InterpolateFrameAt(Owner.Game.Time - startTime);

        var position = Owner.Position.NumericValue;

        position.Z += keyFrame.Height.NumericValue;

        e.Core.PointLight.Draw(
            position,
            keyFrame.Radius.NumericValue,
            keyFrame.Color
        );
    }
}

