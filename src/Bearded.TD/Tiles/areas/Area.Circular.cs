using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Tiles
{
    static partial class Area
    {
        public static IArea Circular(Position2 center, Unit radius) => new CircularArea(center, radius);

        private sealed class CircularArea : IArea
        {
            private readonly Position2 center;
            private readonly Unit radius;

            public CircularArea(Position2 center, Unit radius)
            {
                this.center = center;
                this.radius = radius;
            }

            public bool Contains(Tile tile) =>
                (Level.GetPosition(tile) - center).LengthSquared < radius.Squared;

            public IEnumerable<Tile> Enumerated => Level.TilesWithCenterInCircle(center, radius);

            public IEnumerator<Tile> GetEnumerator() => Enumerated.GetEnumerator();

            public ImmutableHashSet<Tile> ToImmutableHashSet() => Enumerated.ToImmutableHashSet();

            public ImmutableArray<Tile> ToImmutableArray() => Enumerated.ToImmutableArray();
        }
    }
}
