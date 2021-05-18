using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bearded.TD.Tiles
{
    sealed class Area : IEquatable<Area>
    {
        private readonly ImmutableHashSet<Tile> tiles;

        public int Count => tiles.Count;

        public static Area From(IEnumerable<Tile> tiles)
        {
            return tiles switch
            {
                ImmutableHashSet<Tile> hashSet => new Area(hashSet),
                _ => new Area(tiles.ToImmutableHashSet())
            };
        }

        private Area(ImmutableHashSet<Tile> tiles)
        {
            this.tiles = tiles;
        }

        public bool Contains(Tile tile) => tiles.Contains(tile);


        public IEnumerable<Tile> Enumerated => tiles;
        public IEnumerator<Tile> GetEnumerator() => tiles.GetEnumerator();
        public ImmutableHashSet<Tile> ToImmutableHashSet() => tiles;
        public ImmutableArray<Tile> ToImmutableArray() => tiles.ToImmutableArray();

        public Area Erode() => From(tiles.Where(t => t.PossibleNeighbours().All(tiles.Contains)));

        public bool Equals(Area? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return tiles.Count == other.tiles.Count && tiles.All(other.tiles.Contains);
        }

        public override bool Equals(object? obj) => obj is Area other && Equals(other);
        public override int GetHashCode() => tiles.GetHashCode();

        public static bool operator ==(Area? left, Area? right) => Equals(left, right);
        public static bool operator !=(Area? left, Area? right) => !Equals(left, right);
    }
}
