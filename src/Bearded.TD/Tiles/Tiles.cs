using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bearded.TD.Tiles
{
    sealed class Tiles : IEquatable<Tiles>, IEnumerable<Tile>
    {
        private readonly ImmutableHashSet<Tile> tiles;

        public Tiles(IEnumerable<Tile> tiles)
        {
            this.tiles = tiles.ToImmutableHashSet();
        }

        public ImmutableHashSet<Tile> ToImmutableHashSet() => tiles;
        public ImmutableArray<Tile> ToImmutableArray() => tiles.ToImmutableArray();

        public Tiles Erode() => new(tiles.Where(t => t.PossibleNeighbours().All(tiles.Contains)));

        public bool Equals(Tiles? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return tiles.Count == other.tiles.Count && tiles.All(other.tiles.Contains);
        }

        public IEnumerator<Tile> GetEnumerator() => tiles.GetEnumerator();

        public override bool Equals(object? obj) => obj is Tiles other && Equals(other);
        public override int GetHashCode() => tiles.GetHashCode();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static bool operator ==(Tiles? left, Tiles? right) => Equals(left, right);
        public static bool operator !=(Tiles? left, Tiles? right) => !Equals(left, right);
    }
}
