using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Tiles
{
    static partial class Area
    {
        public static IArea Circular(Tile center, Unit radius) => new CircularArea(center, radius);

        private sealed class CircularArea : IArea
        {
            private readonly Tile center;
            private readonly Unit radius;

            public CircularArea(Tile center, Unit radius)
            {
                this.center = center;
                this.radius = radius;
            }

            public bool Contains(Tile tile) =>
                (Level.GetPosition(tile) - Level.GetPosition(center)).LengthSquared < radius.Squared;

            public IEnumerable<Tile> Enumerated => Level.TilesWithCenterInCircle(Level.GetPosition(center), radius);

            public IEnumerator<Tile> GetEnumerator() => Enumerated.GetEnumerator();

            public ImmutableHashSet<Tile> ToImmutableHashSet() => Enumerated.ToImmutableHashSet();

            public ImmutableArray<Tile> ToImmutableArray() => Enumerated.ToImmutableArray();
        }
    }
}
