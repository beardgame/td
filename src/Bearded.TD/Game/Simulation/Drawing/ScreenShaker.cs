using System;
using System.Collections.Generic;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static System.Math;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

interface IScreenShaker
{
    void Shake(ScreenShakeParameters shake);

    Vector2 GetDisplacementAt(Position3 cameraPosition);
}

public record struct ScreenShakeParameters(
    Position3 Origin,
    Direction2 Direction,
    float Strength,
    TimeSpan Duration,
    Frequency Frequency);

sealed class ScreenShaker : IScreenShaker
{
    private record struct Instance(
        ScreenShakeParameters Parameters,
        Instant StartTime,
        Vector2 MaxDisplacement);

    private ITimeSource? timeSource;
    private Instant now => timeSource!.Time;

    private readonly List<Instance> instances = new();

    public void Shake(ScreenShakeParameters shake)
    {
        var instance = new Instance(
            shake,
            now,
            shake.Direction.Vector * shake.Strength
            );
        instances.Add(instance);
    }

    public Vector2 GetDisplacementAt(Position3 cameraPosition)
    {
        var t = now;
        instances.RemoveAll(i => i.StartTime + i.Parameters.Duration <= t);

        var displacement = Vector2.Zero;

        foreach (var instance in instances)
        {
            displacement += getDisplacement(instance, cameraPosition, t);
        }

        return displacement;
    }

    private static Vector2 getDisplacement(Instance instance, Position3 cameraPosition, Instant time)
    {
        var localTime = time - instance.StartTime;
        var wobble = Sin(localTime * instance.Parameters.Frequency * Tau);

        var relativeTime = localTime / instance.Parameters.Duration;
        var timeScalar = Sin(Sqrt(relativeTime) * PI);

        var distance = (cameraPosition - instance.Parameters.Origin).Length;
        var distanceScalar = Min(1, 1 / distance.NumericValue);

        return instance.MaxDisplacement * (float)(wobble * timeScalar * distanceScalar);
    }

    public void SetTimeSource(ITimeSource source)
    {
        if (timeSource != null)
            throw new InvalidOperationException();

        timeSource = source;
    }
}
