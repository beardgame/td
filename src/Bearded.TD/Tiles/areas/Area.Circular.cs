using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

            public int Count => enumerated.Count();

            public bool Contains(Tile tile) =>
                (Level.GetPosition(tile) - center).LengthSquared < radius.Squared;

            private IEnumerable<Tile> enumerated => Level.TilesWithCenterInCircle(center, radius);

            public IEnumerator<Tile> GetEnumerator() => enumerated.GetEnumerator();
        }
    }
}
