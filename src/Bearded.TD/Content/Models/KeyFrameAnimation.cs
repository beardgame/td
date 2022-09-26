using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Drawing.Animation;
using Bearded.TD.Utilities;
using Newtonsoft.Json;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Content.Models;

sealed record KeyFrameAnimation<T>(
    ImmutableArray<T> KeyFrames,
    TimeSpan TotalDuration,
    RepeatMode RepeatMode)
    : IKeyFrameAnimation<T> where T : IKeyFrame<T>
{
    [JsonConstructor]
    public KeyFrameAnimation(ImmutableArray<T> keyFrames, RepeatMode repeatMode = RepeatMode.Once)
        : this(keyFrames, keyFrames.Sum(f => f.Duration.NumericValue).S(), repeatMode)
    {
    }
}
