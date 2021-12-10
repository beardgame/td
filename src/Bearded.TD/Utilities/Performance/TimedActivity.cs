using System;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Utilities.Performance;

readonly struct TimedActivity : IEquatable<TimedActivity>
{
    public Activity Activity { get; }
    public TimeSpan TimeSpan { get; }

    public TimedActivity(Activity activity, TimeSpan timeSpan)
    {
        Activity = activity;
        TimeSpan = timeSpan;
    }

    public bool Equals(TimedActivity other)
        => Activity == other.Activity && TimeSpan.Equals(other.TimeSpan);

    public override bool Equals(object? obj)
        => obj is TimedActivity other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine((int)Activity, TimeSpan);

    public static bool operator ==(TimedActivity left, TimedActivity right)
        => left.Equals(right);

    public static bool operator !=(TimedActivity left, TimedActivity right)
        => !left.Equals(right);
}