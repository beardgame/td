using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.World;

static class FootprintExtensions
{
    public static PositionedFootprint Positioned(this IFootprint footprint, Tile tile) => new(footprint, tile);
}
