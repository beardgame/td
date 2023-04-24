using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

readonly record struct PositionedFootprint(
    IFootprint? Footprint, Tile RootTile, Orientation Orientation = Orientation.Default)
{
    public static IFootprint? Invalid => default;

    public IEnumerable<Tile> OccupiedTiles => Footprint?.OccupiedTiles(RootTile) ?? ImmutableHashSet<Tile>.Empty;
    public Position2 CenterPosition => Footprint?.Center(RootTile) ?? Position2.Zero;

    public bool IsValid(Level level) => Footprint != null && OccupiedTiles.All(level.IsValid);
}
