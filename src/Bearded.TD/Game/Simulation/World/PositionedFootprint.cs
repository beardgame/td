using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World
{
    readonly struct PositionedFootprint
    {
        public Tile RootTile { get; }
        public FootprintGroup? Footprint { get; }
        public int FootprintIndex { get; }

        public Angle Orientation => Footprint?.Orientations[FootprintIndex] ?? Angle.Zero;
        public IEnumerable<Tile> OccupiedTiles =>
            Footprint?.Footprints[FootprintIndex].OccupiedTiles(RootTile) ?? ImmutableHashSet<Tile>.Empty;
        public Position2 CenterPosition => Footprint?.Footprints[FootprintIndex].Center(RootTile) ?? Position2.Zero;

        public PositionedFootprint(FootprintGroup footprint, int index, Tile rootTile)
        {
            RootTile = rootTile;
            Footprint = footprint;
            FootprintIndex = index;
        }

        public bool IsValid(Level level) => Footprint != null && OccupiedTiles.All(level.IsValid);
    }
}
