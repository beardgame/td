using System;
using System.Collections.Generic;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    sealed class AreaTagValues
    {
        private readonly IArea tiles;
        private readonly Dictionary<Tile, double> values = new();

        public AreaTagValues(IArea tiles)
        {
            this.tiles = tiles;
        }

        public double this[Tile tile]
        {
            set
            {
                if (!tiles.Contains(tile))
                    throw new ArgumentOutOfRangeException(nameof(tile), "Cannot set unknown tile.");

                values[tile] = value;
            }
            get
            {
                if (!tiles.Contains(tile))
                    throw new ArgumentOutOfRangeException(nameof(tile), "Cannot get unknown tile.");

                return values.TryGetValue(tile, out var value) ? value : 0;
            }
        }
    }
}
