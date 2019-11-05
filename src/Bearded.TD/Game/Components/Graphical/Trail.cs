using System.Collections;
using System.Collections.Generic;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game.Components.Effects
{
    sealed class Trail : IReadOnlyList<Trail.Part>
    {
        public struct Part
        {
            public Position2 Point { get; }
            public Vector2 Normal { get; }
            public Unit DistanceToPreviousPart { get; }
            public Instant Timeout { get; }

            public Part(Position2 point, Vector2 normal, Unit distanceToPreviousPart, Instant timeout)
            {
                Point = point;
                Normal = normal;
                DistanceToPreviousPart = distanceToPreviousPart;
                Timeout = timeout;
            }
        }

        private readonly List<Part> parts = new List<Part>();
        private readonly float angleThresholdCosine;
        private readonly Squared<Unit> distanceThresholdSquared;

        private int firstPartIndex;
        private int lastPartIndex => parts.Count - 1;
        private int partCount => parts.Count - firstPartIndex;

        public TimeSpan TimeoutDuration { get; }
        
        public Trail(TimeSpan timeoutDuration, Angle? newPartAngleThreshold = null, Unit? newPartDistanceThreshold = null)
        {
            TimeoutDuration = timeoutDuration;
            angleThresholdCosine = (newPartAngleThreshold ?? 5.Degrees()).Cos();
            distanceThresholdSquared = (newPartDistanceThreshold ?? 1.U()).Squared;
        }

        public void Update(Instant currentTime, Position2 parentPosition, bool onlyFadeAway = false)
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

            var previousNormal = ((thisPart.Point - previousPart.Point) / thisPart.DistanceToPreviousPart).PerpendicularRight;
            var previousWeight = thisPart.DistanceToPreviousPart;

            var weight = previousWeight / (previousWeight + nextWeight);

            var normal = Vector2.Lerp(previousNormal, nextNormal, weight).NormalizedSafe();

            parts[lastI - 1] = part(thisPart, normal);
        }

        private void addPartOrUpdateLast(Position2 parentPosition, Instant currentTime)
        {
            // we want 3 parts so we can calculate the andle between the last two segments below
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
            var previousNormal = (previousDifference / previousDifference.Length).PerpendicularRight;
            
            var newLastFixedNormal = (newLastFixedDifference / newLastFixedDistanceSquared.Sqrt()).PerpendicularRight;

            var previousNewAngleCosine = Vector2.Dot(previousNormal, newLastFixedNormal);

            if (previousNewAngleCosine < angleThresholdCosine)
            {
                addNewLastPart(parentPosition, currentTime);
                return;
            }

            var lastPart = parts[lastPartIndex];

            var lastDifference = lastPart.Point - lastFixedPart.Point;
            var lastNormal = (lastDifference / lastDifference.Length).PerpendicularRight;
            
            var newLastDifference = parentPosition - lastPart.Point;
            var newLastNormal = (newLastDifference / newLastDifference.Length).PerpendicularRight;
            
            var lastNewAngleCosine = Vector2.Dot(lastNormal, newLastNormal);
            
            if (lastNewAngleCosine < angleThresholdCosine)
            {
                addNewLastPart(parentPosition, currentTime);
                return;
            }

            updateLastPart(parentPosition, currentTime);
        }

        private void updateLastPart(Position2 parentPosition, Instant currentTime)
        {
            parts.RemoveAt(lastPartIndex);

            addNewLastPart(parentPosition, currentTime);
        }

        private void addNewLastPart(Position2 parentPosition, Instant currentTime)
        {
            var part = createNewLastPart(parentPosition, currentTime);
            
            parts.Add(part);
        }

        private Part createNewLastPart(Position2 parentPosition, Instant currentTime)
        {
            var lastPart = parts[lastPartIndex];

            var difference = parentPosition - lastPart.Point;
            var distance = difference.Length;
            var vector = difference / distance;
            var normal = vector.PerpendicularRight;

            var p = part(parentPosition, normal, distance, currentTime + TimeoutDuration);
            return p;
        }

        private void addFirstPart(Position2 parentPosition, Instant currentTime)
        {
            parts.Add(part(parentPosition, currentTime + TimeoutDuration));
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

        private static Part part(Position2 point, Vector2 normal, Unit distanceToPreviousPart, Instant timeOut)
            => new Part(point, normal, distanceToPreviousPart, timeOut);
        
        private static Part part(Position2 position, Instant timeOut)
            => new Part(position, Vector2.Zero, Unit.Zero, timeOut);

        private static Part part(Part part, Vector2 normal)
            => new Part(part.Point, normal, part.DistanceToPreviousPart, part.Timeout);


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
}
