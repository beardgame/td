using System.Collections;
using System.Collections.Generic;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Drawing;

sealed class TrailTracer : IReadOnlyList<TrailTracer.Part>
{
    public struct Part
    {
        public Position3 Point { get; }
        public Vector2 Normal { get; }
        public Unit DistanceToPreviousPart { get; }
        public Instant Timeout { get; }

        public Part(Position3 point, Vector2 normal, Unit distanceToPreviousPart, Instant timeout)
        {
            Point = point;
            Normal = normal;
            DistanceToPreviousPart = distanceToPreviousPart;
            Timeout = timeout;
        }
    }

    private readonly List<Part> parts = new();
    private readonly float angleThresholdCosine;
    private readonly Squared<Unit> distanceThresholdSquared;

    private int firstPartIndex;
    private int lastPartIndex => parts.Count - 1;
    private int partCount => parts.Count - firstPartIndex;

    private TimeSpan timeoutDuration { get; }

    public TrailTracer(TimeSpan timeoutDuration, Angle? newPartAngleThreshold = null, Unit? newPartDistanceThreshold = null)
    {
        this.timeoutDuration = timeoutDuration;
        angleThresholdCosine = (newPartAngleThreshold ?? 5.Degrees()).Cos();
        distanceThresholdSquared = (newPartDistanceThreshold ?? 1.U()).Squared;
    }

    public void Update(Instant currentTime, Position3 parentPosition, bool onlyFadeAway = false)
    {
        if (parts.Count == 0)
        {
            addFirstPart(parentPosition, currentTime);
            return;
        }

        if (!onlyFadeAway)
        {
            addPartOrUpdateLast(parentPosition, currentTime);

            updateLastPartNormals();
        }

        timeoutParts(currentTime);
    }

    private void updateLastPartNormals()
    {
        if (parts.Count < 3)
            return;

        var lastI = lastPartIndex;

        var nextPart = parts[lastI];
        var thisPart = parts[lastI - 1];
        var previousPart = parts[lastI - 2];

        var nextNormal = nextPart.Normal;
        var nextWeight = nextPart.DistanceToPreviousPart;

        var previousNormal = ((thisPart.Point - previousPart.Point) / thisPart.DistanceToPreviousPart).Xy.PerpendicularRight;
        var previousWeight = thisPart.DistanceToPreviousPart;

        var weight = previousWeight / (previousWeight + nextWeight);

        var normal = Vector2.Lerp(previousNormal, nextNormal, weight).NormalizedSafe();

        parts[lastI - 1] = part(thisPart, normal);
    }

    private void addPartOrUpdateLast(Position3 parentPosition, Instant currentTime)
    {
        // we want 3 parts so we can calculate the angle between the last two segments below
        if (parts.Count < 3)
        {
            addNewLastPart(parentPosition, currentTime);
            return;
        }

        var lastFixedPart = parts[lastPartIndex - 1];

        var newLastFixedDifference = parentPosition - lastFixedPart.Point;
        var newLastFixedDistanceSquared = newLastFixedDifference.LengthSquared;

        if (newLastFixedDistanceSquared > distanceThresholdSquared)
        {
            addNewLastPart(parentPosition, currentTime);
            return;
        }

        var previousFixedPart = parts[lastPartIndex - 2];

        var previousDifference = lastFixedPart.Point - previousFixedPart.Point;
        var previousNormal = (previousDifference / previousDifference.Length).Xy.PerpendicularRight;

        var newLastFixedNormal = (newLastFixedDifference / newLastFixedDistanceSquared.Sqrt()).Xy.PerpendicularRight;

        var previousNewAngleCosine = Vector2.Dot(previousNormal, newLastFixedNormal);

        if (previousNewAngleCosine < angleThresholdCosine)
        {
            addNewLastPart(parentPosition, currentTime);
            return;
        }

        var lastPart = parts[lastPartIndex];

        var lastDifference = lastPart.Point - lastFixedPart.Point;
        var lastNormal = (lastDifference / lastDifference.Length).Xy.PerpendicularRight;

        var newLastDifference = parentPosition - lastPart.Point;
        var newLastNormal = (newLastDifference / newLastDifference.Length).Xy.PerpendicularRight;

        var lastNewAngleCosine = Vector2.Dot(lastNormal, newLastNormal);

        if (lastNewAngleCosine < angleThresholdCosine)
        {
            addNewLastPart(parentPosition, currentTime);
            return;
        }

        updateLastPart(parentPosition, currentTime);
    }

    private void updateLastPart(Position3 parentPosition, Instant currentTime)
    {
        parts.RemoveAt(lastPartIndex);

        addNewLastPart(parentPosition, currentTime);
    }

    private void addNewLastPart(Position3 parentPosition, Instant currentTime)
    {
        var part = createNewLastPart(parentPosition, currentTime);

        parts.Add(part);
    }

    private Part createNewLastPart(Position3 parentPosition, Instant currentTime)
    {
        var lastPart = parts[lastPartIndex];

        var difference = parentPosition - lastPart.Point;
        var distance = difference.Length;
        var vector = difference / distance;
        var normal = vector.Xy.PerpendicularRight;

        var p = part(parentPosition, normal, distance, currentTime + timeoutDuration);
        return p;
    }

    private void addFirstPart(Position3 parentPosition, Instant currentTime)
    {
        parts.Add(part(parentPosition, currentTime + timeoutDuration));
    }

    private void timeoutParts(Instant currentTime)
    {
        var previousFirstPartIndex = firstPartIndex;

        while (partCount > 1)
        {
            var firstPart = parts[firstPartIndex];

            if (currentTime <= firstPart.Timeout)
            {
                if (previousFirstPartIndex != firstPartIndex)
                {
                    // keep one timed out part around for better fading purposes
                    firstPartIndex--;
                }
                break;
            }

            firstPartIndex++;
        }

        if (firstPartIndex * 2 > parts.Count)
        {
            parts.RemoveRange(0, firstPartIndex);
            firstPartIndex = 0;
        }
    }

    private static Part part(Position3 point, Vector2 normal, Unit distanceToPreviousPart, Instant timeOut)
        => new(point, normal, distanceToPreviousPart, timeOut);

    private static Part part(Position3 position, Instant timeOut)
        => new(position, Vector2.Zero, Unit.Zero, timeOut);

    private static Part part(Part part, Vector2 normal) =>
        new(part.Point, normal, part.DistanceToPreviousPart, part.Timeout);


    public IEnumerator<Part> GetEnumerator()
    {
        var limit = firstPartIndex + partCount;
        for (var i = firstPartIndex; i < limit; i++)
            yield return parts[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => partCount;

    public Part this[int index] => parts[firstPartIndex + index];
}
