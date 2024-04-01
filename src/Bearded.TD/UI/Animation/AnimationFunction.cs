using System;
using Bearded.Graphics;
using Bearded.TD.UI.Controls;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Animation;

readonly record struct AnimationFunction<TState>(
    Func<TState, TimeSpan, AnimationState> Update,
    Action<TState>? Start = null,
    Action<TState>? End = null)
{
    public AnimationFunction<TNewState> Substitute<TNewState>(Func<TNewState, TState> transform)
    {
        var inner = this;

        return new AnimationFunction<TNewState>(
            Update: (s, t) => inner.Update(transform(s), t),
            Start: inner.Start == null ? null : s => inner.Start(transform(s)),
            End: inner.End == null ? null : s => inner.End(transform(s))
        );
    }
    public AnimationFunction<(T1, T2)> Substitute<T1, T2>(Func<T1, T2, TState> transform)
    {
        var inner = this;

        return new AnimationFunction<(T1, T2)>(
            Update: (s, t) => inner.Update(transform(s.Item1, s.Item2), t),
            Start: inner.Start == null ? null : s => inner.Start(transform(s.Item1, s.Item2)),
            End: inner.End == null ? null : s => inner.End(transform(s.Item1, s.Item2))
        );
    }
}

static class AnimationFunction
{
    public static AnimationFunction<Void> ComplexShapeFillColorFromCurrentTo(TimeSpan duration, ComplexShapeControl box, Color to)
    {
        var from = box.Fill.Color.Definition.Color;
        return ComplexShapeFillColorFromTo(duration).Substitute((Void _) => (box, from, to));
    }

    public static AnimationFunction<(ComplexShapeControl, Color from, Color to)> ComplexShapeFillColorFromTo(TimeSpan duration)
    {
        return ZeroToOne<(ComplexShapeControl box, Color from, Color to)>(
            duration,
            (state, t) => state.box.Fill = Color.Lerp(state.from, state.to, t)
        );
    }

    public static AnimationFunction<(BackgroundBox, Color from, Color to)> BackgroundBoxColorFromTo(TimeSpan duration)
    {
        return ZeroToOne<(BackgroundBox box, Color from, Color to)>(
            duration,
            (state, t) => state.box.Color = Color.Lerp(state.from, state.to, t)
        );
    }

    public static AnimationFunction<(Action<Color> set, Color from, Color to)> ColorFromTo(TimeSpan duration)
    {
        return ZeroToOne<(Action<Color> set, Color from, Color to)>(
            duration,
            (state, t) => state.set(Color.Lerp(state.from, state.to, t))
        );
    }

    public static AnimationFunction<Void> ZeroToOne(TimeSpan duration, Action<float> update, Action? end = null)
        => new(
            Update: (_, time) =>
            {
                var t = MathF.Min(1, (float)(time / duration));
                update(t);
                return t < 1
                    ? AnimationState.Continue
                    : AnimationState.Ended;
            },
            Start: _ => update(0),
            End: _ =>
            {
                update(1);
                end?.Invoke();
            }
        );

    public static AnimationFunction<TState> ZeroToOne<TState>(
        TimeSpan duration, Action<TState, float> update, Action<TState>? end = null)
        => new(
            Update: (state, time) =>
            {
                var t = MathF.Min(1, (float)(time / duration));
                update(state, t);
                return t < 1
                    ? AnimationState.Continue
                    : AnimationState.Ended;
            },
            Start: state => update(state, 0),
            End: state =>
            {
                update(state, 1);
                end?.Invoke(state);
            }
        );
}
