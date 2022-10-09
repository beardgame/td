using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Animation;

interface IKeyFrameAnimation<T>
    where T : IKeyFrame<T>
{
    ImmutableArray<T> KeyFrames { get; }
    TimeSpan TotalDuration { get; }
    RepeatMode RepeatMode { get; }
    double TimeScale { get; }

    public T InterpolateFrameAt(TimeSpan time)
    {
        time /= TimeScale;

        if (time >= TotalDuration)
        {
            switch (RepeatMode)
            {
                case RepeatMode.Once:
                    return KeyFrames.Last();
                case RepeatMode.Loop:
                    time = (time.NumericValue % TotalDuration.NumericValue).S();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        var firstFrame = KeyFrames.First();
        if (time < firstFrame.Duration)
            return firstFrame;

        for (var i = 1; i < KeyFrames.Length; i++)
        {
            time -= firstFrame.Duration;
            var secondFrame = KeyFrames[i];

            if (time < secondFrame.Duration)
                return firstFrame.InterpolateTowards(secondFrame, time / secondFrame.Duration);

            firstFrame = secondFrame;
        }

        return firstFrame;
    }
}
