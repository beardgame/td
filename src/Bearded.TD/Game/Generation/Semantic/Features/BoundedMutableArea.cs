using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    sealed class BoundedMutableArea : IArea
    {
        private readonly IArea bounds;
        private readonly HashSet<Tile> tiles;

        public BoundedMutableArea(IArea bounds)
        {
            this.bounds = bounds;
            tiles = new HashSet<Tile>(bounds.Enumerated);
        }

        public int Count => tiles.Count;

        public bool Contains(Tile tile) => tiles.Contains(tile);

        public void Add(Tile tile)
        {
            if (!bounds.Contains(tile))
                throw new ArgumentOutOfRangeException(nameof(tile), "Cannot add tile out of bounds.");

            tiles.Add(tile);
        }

        public void Remove(Tile tile) => tiles.Remove(tile);

        public void RemoveAll() => tiles.Clear();

        public void Reset()
        {
            foreach (var tile in bounds)
            {
                tiles.Add(tile);
            }
        }

        public IEnumerator<Tile> GetEnumerator() => tiles.GetEnumerator();

        IEnumerable<Tile> IArea.Enumerated => tiles.AsReadOnlyEnumerable();

        ImmutableHashSet<Tile> IArea.ToImmutableHashSet() => tiles.ToImmutableHashSet();

        ImmutableArray<Tile> IArea.ToImmutableArray() => tiles.ToImmutableArray();
    }
}
