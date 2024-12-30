using System;
using Bearded.TD.UI.Animation;
using Bearded.TD.Utilities;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.UI.Controls;

sealed class AnimatedNumberLabel : Label, IDisposable
{
    public readonly record struct Configuration(
        double MinChangeToAnimate,
        TimeSpan MinAnimationTime,
        TimeSpan MaxAnimationTime,
        bool FlipScrollDirection = false,
        bool ShowPlusSign = false,
        bool RenderZero = true
    );

    public Configuration Config { get; }
    public double CurrentValue { get; private set; }

    private readonly Animations animations;
    private readonly IReadonlyBinding<double> target;

    private IAnimationController? animation;

    public AnimatedNumberLabel(
        Configuration config,
        Animations animations,
        IReadonlyBinding<double> targetValue)
    {
        Config = config;
        this.animations = animations;
        target = targetValue;
        setCurrentValue(target.Value);
        target.SourceUpdated += updateCurrentValue;
        target.ControlUpdated += updateCurrentValue;
        Text = null!;
    }

    private void updateCurrentValue(double value)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (value == CurrentValue)
            return;

        animation?.Cancel();

        var absoluteDifference = Math.Abs(CurrentValue - value);

        if (absoluteDifference < Config.MinChangeToAnimate)
        {
            setCurrentValue(value);
            return;
        }

        var animationStart = CurrentValue;
        var animationEnd = value;

        var duration = Config.MaxAnimationTime
            - (Config.MaxAnimationTime - Config.MinAnimationTime)
            * 1 / (absoluteDifference / 10 + 1);

        animation = animations.Start(AnimationFunction.ZeroToOne(duration,
            t =>
            {
                t = Interpolate.Hermite(0, 0.5f, 1, -0.15f, t);
                var newValue = animationStart + (animationEnd - animationStart) * t;
                setCurrentValue(newValue);
            },
            () => setCurrentValue(value)
        ));
    }

    private void setCurrentValue(double value)
    {
        CurrentValue = value;
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

    public void Dispose()
    {
        target.SourceUpdated -= updateCurrentValue;
        target.ControlUpdated -= updateCurrentValue;
    }
}
