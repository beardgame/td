using System.Collections;
using System.Collections.Generic;
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

        private int firstPartIndex;
        private int lastPartIndex => parts.Count - 1;
        private int partCount => parts.Count - firstPartIndex;

        public TimeSpan TimeoutDuration { get; }
        
        public Trail(TimeSpan timeoutDuration)
        {
            TimeoutDuration = timeoutDuration;
        }

        public void Update(Instant currentTime, Position2 parentPosition)
        {
            if (parts.Count == 0)
            {
                addFirstPart(parentPosition, currentTime);
                return;
            }

            // TODO: only update last part if some min angle/distance not crossed
            addPart(parentPosition, currentTime);

            // TODO: recalculate normals of last two/three points

            timeoutParts(currentTime);
        }

        private void addPart(Position2 parentPosition, Instant currentTime)
        {
            var lastPart = parts[lastPartIndex];

            var difference = parentPosition - lastPart.Point;
            var distance = difference.Length;
            var vector = difference / distance;
            var normal = vector.PerpendicularRight;

            parts.Add(part(parentPosition, normal, distance, currentTime + TimeoutDuration));
        }

        private void addFirstPart(Position2 parentPosition, Instant currentTime)
        {
            parts.Add(part(parentPosition, currentTime + TimeoutDuration));
        }

        private void timeoutParts(Instant currentTime)
        {
            while (partCount > 1)
            {
                var firstPart = parts[firstPartIndex];

                if (currentTime <= firstPart.Timeout)
                    break;

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
