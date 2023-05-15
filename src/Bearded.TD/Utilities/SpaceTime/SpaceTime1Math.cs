using System;

namespace Bearded.TD.Utilities.SpaceTime;

static class SpaceTime1Math
{
    public static T Min<T>(T left, T right) where T : IMeasure1 =>
        left.NumericValue <= right.NumericValue ? left : right;

    public static T Max<T>(T left, T right) where T : IMeasure1 =>
        left.NumericValue >= right.NumericValue ? left : right;

    public static T Clamp<T>(T val, T min, T max) where T : IMeasure1 => Min(Max(min, val), max);

    public static int Sign<T>(T val) where T : IMeasure1 => Math.Sign(val.NumericValue);
}

static class SpaceTime1MathF
{
    public static T Min<T>(T left, T right) where T : IMeasure1F =>
        left.NumericValue <= right.NumericValue ? left : right;

    public static T Max<T>(T left, T right) where T : IMeasure1F =>
        left.NumericValue >= right.NumericValue ? left : right;

    public static T Clamp<T>(T val, T min, T max) where T : IMeasure1F => Min(Max(min, val), max);

    public static int Sign<T>(T val) where T : IMeasure1F => MathF.Sign(val.NumericValue);
}

static class DiscreteSpaceTime1Math
{
    public static T Min<T>(T left, T right) where T : IDiscreteMeasure1 =>
        left.NumericValue <= right.NumericValue ? left : right;

    public static T Max<T>(T left, T right) where T : IDiscreteMeasure1 =>
        left.NumericValue >= right.NumericValue ? left : right;

    public static T Clamp<T>(T val, T min, T max) where T : IDiscreteMeasure1 => Min(Max(min, val), max);

    public static int Sign<T>(T val) where T : IDiscreteMeasure1 => Math.Sign(val.NumericValue);
}
